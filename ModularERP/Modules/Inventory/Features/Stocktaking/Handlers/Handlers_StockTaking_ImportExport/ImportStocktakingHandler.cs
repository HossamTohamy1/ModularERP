using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_ImportExport;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_ImportExport;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Shared.Interfaces;
using OfficeOpenXml;
using System.ComponentModel;
using System.Text;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTaking_ImportExport
{
    public class ImportStocktakingHandler : IRequestHandler<ImportStocktakingCommand, ImportStocktakingResultDto>
    {
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepo;
        private readonly IGeneralRepository<StocktakingLine> _lineRepo;
        private readonly IGeneralRepository<Product> _productRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<ImportStocktakingHandler> _logger;

        public ImportStocktakingHandler(
            IGeneralRepository<StocktakingHeader> stocktakingRepo,
            IGeneralRepository<StocktakingLine> lineRepo,
            IGeneralRepository<Product> productRepo,
            IMapper mapper,
            ILogger<ImportStocktakingHandler> logger)
        {
            _stocktakingRepo = stocktakingRepo;
            _lineRepo = lineRepo;
            _productRepo = productRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ImportStocktakingResultDto> Handle(
            ImportStocktakingCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting import for StocktakingId: {StocktakingId}", request.StocktakingId);

            var stocktaking = await _stocktakingRepo
                .Get(s => s.Id == request.StocktakingId && !s.IsDeleted)
                .Select(s => new
                {
                    s.Id,
                    s.Status,
                    s.WarehouseId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (stocktaking == null)
            {
                throw new NotFoundException(
                    "Stocktaking not found",
                    FinanceErrorCode.NotFound);
            }

            if (stocktaking.Status != Common.Enum.Inventory_Enum.StocktakingStatus.Draft &&
                stocktaking.Status != Common.Enum.Inventory_Enum.StocktakingStatus.Counting)
            {
                throw new BusinessLogicException(
                    "Cannot import to stocktaking that is not in Draft or Counting status",
                    "Inventory");
            }

            var result = new ImportStocktakingResultDto
            {
                StocktakingId = request.StocktakingId
            };

            var importItems = await ParseFileAsync(request.File, cancellationToken);
            result.TotalRecords = importItems.Count;

            var productCodes = importItems.Select(i => i.ProductCode.ToUpper()).Distinct().ToList();
            var products = await _productRepo
                .Get(p => productCodes.Contains(p.SKU.ToUpper()) && !p.IsDeleted)
                .Select(p => new
                {
                    p.Id,
                    p.SKU,
                    p.Name
                })
                .ToListAsync(cancellationToken);

            var existingLines = await _lineRepo
                .Get(l => l.StocktakingId == request.StocktakingId && !l.IsDeleted)
                .Select(l => l.ProductId)
                .ToListAsync(cancellationToken);

            var linesToAdd = new List<StocktakingLine>();
            var rowNumber = 1;

            foreach (var item in importItems)
            {
                rowNumber++;

                var product = products.FirstOrDefault(p =>
                    p.SKU.Equals(item.ProductCode, StringComparison.OrdinalIgnoreCase));

                if (product == null)
                {
                    result.FailedRecords++;
                    result.Errors.Add(new ImportErrorDto
                    {
                        RowNumber = rowNumber,
                        ProductCode = item.ProductCode,
                        ErrorMessage = $"Product with SKU '{item.ProductCode}' not found"
                    });
                    continue;
                }

                if (existingLines.Contains(product.Id))
                {
                    result.FailedRecords++;
                    result.Errors.Add(new ImportErrorDto
                    {
                        RowNumber = rowNumber,
                        ProductCode = item.ProductCode,
                        ErrorMessage = $"Product '{product.Name}' already exists in this stocktaking"
                    });
                    continue;
                }

                if (item.PhysicalQty < 0)
                {
                    result.FailedRecords++;
                    result.Errors.Add(new ImportErrorDto
                    {
                        RowNumber = rowNumber,
                        ProductCode = item.ProductCode,
                        ErrorMessage = "Physical quantity cannot be negative"
                    });
                    continue;
                }

                var systemQty = await GetSystemQuantityAsync(
                    product.Id,
                    stocktaking.WarehouseId,
                    cancellationToken);

                var line = new StocktakingLine
                {
                    Id = Guid.NewGuid(),
                    StocktakingId = request.StocktakingId,
                    ProductId = product.Id,
                    PhysicalQty = item.PhysicalQty,
                    SystemQtySnapshot = systemQty,
                    VarianceQty = item.PhysicalQty - systemQty,
                    Note = item.Note,
                    CreatedAt = DateTime.UtcNow
                };

                linesToAdd.Add(line);
                existingLines.Add(product.Id);

                result.ImportedLines.Add(new ImportedLineDto
                {
                    LineId = line.Id,
                    ProductName = product.Name,
                    ProductSKU = product.SKU,
                    PhysicalQty = line.PhysicalQty,
                    SystemQtySnapshot = line.SystemQtySnapshot,
                    VarianceQty = line.VarianceQty,
                    Note = line.Note
                });

                result.SuccessfulRecords++;
            }

            if (linesToAdd.Any())
            {
                await _lineRepo.AddRangeAsync(linesToAdd);
                await _lineRepo.SaveChanges();

                _logger.LogInformation(
                    "Successfully imported {Count} lines for StocktakingId: {StocktakingId}",
                    linesToAdd.Count,
                    request.StocktakingId);
            }

            return result;
        }

        private async Task<List<ImportLineItemDto>> ParseFileAsync(
            IFormFile file,
            CancellationToken cancellationToken)
        {
            var items = new List<ImportLineItemDto>();
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream, cancellationToken);
            stream.Position = 0;

            if (extension == ".csv")
            {
                items = await ParseCsvAsync(stream, cancellationToken);
            }
            else
            {
                items = await ParseExcelAsync(stream, cancellationToken);
            }

            return items;
        }

        private async Task<List<ImportLineItemDto>> ParseCsvAsync(
            Stream stream,
            CancellationToken cancellationToken)
        {
            var items = new List<ImportLineItemDto>();

            using var reader = new StreamReader(stream, Encoding.UTF8);
            await reader.ReadLineAsync(); // Skip header

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = line.Split(',');
                if (values.Length < 2) continue;

                items.Add(new ImportLineItemDto
                {
                    ProductCode = values[0].Trim(),
                    PhysicalQty = decimal.TryParse(values[1].Trim(), out var qty) ? qty : 0,
                    Note = values.Length > 2 ? values[2].Trim() : null,
                    ImageUrl = values.Length > 3 ? values[3].Trim() : null
                });
            }

            return items;
        }

        private async Task<List<ImportLineItemDto>> ParseExcelAsync(
            Stream stream,
            CancellationToken cancellationToken)
        {
            var items = new List<ImportLineItemDto>();
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension?.Rows ?? 0;

            for (int row = 2; row <= rowCount; row++)
            {
                var productCode = worksheet.Cells[row, 1].Text?.Trim();
                if (string.IsNullOrEmpty(productCode)) continue;

                items.Add(new ImportLineItemDto
                {
                    ProductCode = productCode,
                    PhysicalQty = decimal.TryParse(worksheet.Cells[row, 2].Text, out var qty) ? qty : 0,
                    Note = worksheet.Cells[row, 3].Text?.Trim(),
                    ImageUrl = worksheet.Cells[row, 4].Text?.Trim()
                });
            }

            return items;
        }

        private async Task<decimal> GetSystemQuantityAsync(
            Guid productId,
            Guid warehouseId,
            CancellationToken cancellationToken)
        {
            // Query stock snapshots first (if exists for this stocktaking)
            var snapshot = await _stocktakingRepo
                .GetAll()
                .Where(s => !s.IsDeleted)
                .SelectMany(s => s.Snapshots)
                .Where(snap => snap.ProductId == productId && !snap.IsDeleted)
                .Select(snap => snap.QtyAtStart)
                .FirstOrDefaultAsync(cancellationToken);

            if (snapshot > 0)
            {
                return snapshot;
            }

            // Fallback: Query from inventory balance/stock table
            // TODO: Replace with actual inventory balance query
            // Example:
            // var balance = await _inventoryBalanceRepo
            //     .Get(b => b.ProductId == productId && b.WarehouseId == warehouseId)
            //     .Select(b => b.OnHandQty)
            //     .FirstOrDefaultAsync(cancellationToken);

            return 0;
        }
    }
}