using MediatR;
using Microsoft.Extensions.Logging;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_ImportExport;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTaking_ImportExport;
using ModularERP.Shared.Interfaces;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using Microsoft.EntityFrameworkCore;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTaking_ImportExport
{
    public class ExportStocktakingHandler : IRequestHandler<ExportStocktakingQuery, ExportStocktakingResultDto>
    {
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepo;
        private readonly IGeneralRepository<StocktakingLine> _lineRepo;
        private readonly ILogger<ExportStocktakingHandler> _logger;

        public ExportStocktakingHandler(
            IGeneralRepository<StocktakingHeader> stocktakingRepo,
            IGeneralRepository<StocktakingLine> lineRepo,
            ILogger<ExportStocktakingHandler> logger)
        {
            _stocktakingRepo = stocktakingRepo;
            _lineRepo = lineRepo;
            _logger = logger;
        }

        public async Task<ExportStocktakingResultDto> Handle(
            ExportStocktakingQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Exporting stocktaking data for StocktakingId: {StocktakingId}", request.StocktakingId);

            var stocktaking = await _stocktakingRepo
                .Get(s => s.Id == request.StocktakingId && !s.IsDeleted)
                .Select(s => new
                {
                    s.Id,
                    s.Number,
                    s.DateTime,
                    WarehouseName = s.Warehouse != null ? s.Warehouse.Name : string.Empty
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (stocktaking == null)
            {
                throw new NotFoundException(
                    "Stocktaking not found",
                    FinanceErrorCode.NotFound);
            }

            var lines = await _lineRepo
                .Get(l => l.StocktakingId == request.StocktakingId && !l.IsDeleted)
                .Select(l => new
                {
                    ProductSKU = l.Product != null ? l.Product.SKU : string.Empty,
                    ProductName = l.Product != null ? l.Product.Name : string.Empty,
                    l.PhysicalQty,
                    l.SystemQtySnapshot,
                    l.SystemQtyAtPost,
                    l.VarianceQty,
                    l.ValuationCost,
                    l.VarianceValue,
                    l.Note,
                    l.ImagePath
                })
                .OrderBy(l => l.ProductSKU)
                .ToListAsync(cancellationToken);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Stocktaking");

            // Header
            worksheet.Cells["A1"].Value = "Stocktaking Number:";
            worksheet.Cells["B1"].Value = stocktaking.Number;
            worksheet.Cells["A2"].Value = "Warehouse:";
            worksheet.Cells["B2"].Value = stocktaking.WarehouseName;
            worksheet.Cells["A3"].Value = "Date:";
            worksheet.Cells["B3"].Value = stocktaking.DateTime.ToString("yyyy-MM-dd HH:mm");

            // Column Headers
            int headerRow = 5;
            worksheet.Cells[headerRow, 1].Value = "Product SKU";
            worksheet.Cells[headerRow, 2].Value = "Product Name";
            worksheet.Cells[headerRow, 3].Value = "Physical Qty";
            worksheet.Cells[headerRow, 4].Value = "System Qty (Snapshot)";
            worksheet.Cells[headerRow, 5].Value = "System Qty (At Post)";
            worksheet.Cells[headerRow, 6].Value = "Variance Qty";
            worksheet.Cells[headerRow, 7].Value = "Valuation Cost";
            worksheet.Cells[headerRow, 8].Value = "Variance Value";
            worksheet.Cells[headerRow, 9].Value = "Note";
            worksheet.Cells[headerRow, 10].Value = "Image Path";

            // Style header
            using (var range = worksheet.Cells[headerRow, 1, headerRow, 10])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Data rows
            int row = headerRow + 1;
            foreach (var line in lines)
            {
                worksheet.Cells[row, 1].Value = line.ProductSKU;
                worksheet.Cells[row, 2].Value = line.ProductName;
                worksheet.Cells[row, 3].Value = line.PhysicalQty;
                worksheet.Cells[row, 4].Value = line.SystemQtySnapshot;
                worksheet.Cells[row, 5].Value = line.SystemQtyAtPost;
                worksheet.Cells[row, 6].Value = line.VarianceQty;
                worksheet.Cells[row, 7].Value = line.ValuationCost;
                worksheet.Cells[row, 8].Value = line.VarianceValue;
                worksheet.Cells[row, 9].Value = line.Note;
                worksheet.Cells[row, 10].Value = line.ImagePath;

                // Color code variance
                if (line.VarianceQty < 0)
                {
                    worksheet.Cells[row, 6].Style.Font.Color.SetColor(Color.Red);
                }
                else if (line.VarianceQty > 0)
                {
                    worksheet.Cells[row, 6].Style.Font.Color.SetColor(Color.Green);
                }

                row++;
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var fileContent = package.GetAsByteArray();
            var fileName = $"Stocktaking_{stocktaking.Number}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

            _logger.LogInformation("Successfully exported {Count} lines", lines.Count);

            return new ExportStocktakingResultDto
            {
                FileContent = fileContent,
                FileName = fileName,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
        }
    }

    // Export blank template with system quantities
    public class ExportStocktakingTemplateHandler : IRequestHandler<ExportStocktakingTemplateQuery, ExportStocktakingResultDto>
    {
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepo;
        private readonly IGeneralRepository<StocktakingLine> _lineRepo;
        private readonly ILogger<ExportStocktakingTemplateHandler> _logger;

        public ExportStocktakingTemplateHandler(
            IGeneralRepository<StocktakingHeader> stocktakingRepo,
            IGeneralRepository<StocktakingLine> lineRepo,
            ILogger<ExportStocktakingTemplateHandler> logger)
        {
            _stocktakingRepo = stocktakingRepo;
            _lineRepo = lineRepo;
            _logger = logger;
        }

        public async Task<ExportStocktakingResultDto> Handle(
            ExportStocktakingTemplateQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Exporting stocktaking template for StocktakingId: {StocktakingId}", request.StocktakingId);

            var stocktaking = await _stocktakingRepo
                .Get(s => s.Id == request.StocktakingId && !s.IsDeleted)
                .Select(s => new
                {
                    s.Id,
                    s.Number,
                    s.DateTime,
                    WarehouseName = s.Warehouse != null ? s.Warehouse.Name : string.Empty
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (stocktaking == null)
            {
                throw new NotFoundException(
                    "Stocktaking not found",
                    FinanceErrorCode.NotFound);
            }

            var existingLines = await _lineRepo
                .Get(l => l.StocktakingId == request.StocktakingId && !l.IsDeleted)
                .Select(l => new
                {
                    ProductSKU = l.Product != null ? l.Product.SKU : string.Empty,
                    ProductName = l.Product != null ? l.Product.Name : string.Empty,
                    l.SystemQtySnapshot
                })
                .OrderBy(l => l.ProductSKU)
                .ToListAsync(cancellationToken);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Stocktaking Template");

            // Header
            worksheet.Cells["A1"].Value = "Stocktaking Number:";
            worksheet.Cells["B1"].Value = stocktaking.Number;
            worksheet.Cells["C1"].Value = "Instructions: Fill in Physical Qty column";
            worksheet.Cells["A2"].Value = "Warehouse:";
            worksheet.Cells["B2"].Value = stocktaking.WarehouseName;
            worksheet.Cells["A3"].Value = "Date:";
            worksheet.Cells["B3"].Value = stocktaking.DateTime.ToString("yyyy-MM-dd HH:mm");

            // Column Headers
            int headerRow = 5;
            worksheet.Cells[headerRow, 1].Value = "Product SKU*";
            worksheet.Cells[headerRow, 2].Value = "Product Name";
            worksheet.Cells[headerRow, 3].Value = "System Qty (Reference)";
            worksheet.Cells[headerRow, 4].Value = "Physical Qty*";
            worksheet.Cells[headerRow, 5].Value = "Note";
            worksheet.Cells[headerRow, 6].Value = "Image URL";

            // Style header
            using (var range = worksheet.Cells[headerRow, 1, headerRow, 6])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
                range.Style.Font.Color.SetColor(Color.White);
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Add instructions row
            int instructionRow = headerRow + 1;
            worksheet.Cells[instructionRow, 1].Value = "Required field";
            worksheet.Cells[instructionRow, 4].Value = "Enter counted quantity here";
            using (var range = worksheet.Cells[instructionRow, 1, instructionRow, 6])
            {
                range.Style.Font.Italic = true;
                range.Style.Font.Color.SetColor(Color.Gray);
            }

            // Data rows
            int row = instructionRow + 1;
            foreach (var line in existingLines)
            {
                worksheet.Cells[row, 1].Value = line.ProductSKU;
                worksheet.Cells[row, 2].Value = line.ProductName;
                worksheet.Cells[row, 3].Value = line.SystemQtySnapshot;
                // Physical Qty column left empty for user input
                worksheet.Cells[row, 4].Value = "";
                worksheet.Cells[row, 5].Value = "";
                worksheet.Cells[row, 6].Value = "";

                // Lock reference columns
                worksheet.Cells[row, 1, row, 3].Style.Locked = true;
                worksheet.Cells[row, 1, row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 1, row, 3].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                row++;
            }

            // Set column widths
            worksheet.Column(1).Width = 15;
            worksheet.Column(2).Width = 30;
            worksheet.Column(3).Width = 20;
            worksheet.Column(4).Width = 15;
            worksheet.Column(5).Width = 30;
            worksheet.Column(6).Width = 30;

            // Protect worksheet
            worksheet.Protection.IsProtected = true;
            worksheet.Protection.AllowSelectLockedCells = true;
            worksheet.Protection.AllowSelectUnlockedCells = true;

            var fileContent = package.GetAsByteArray();
            var fileName = $"Stocktaking_Template_{stocktaking.Number}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

            _logger.LogInformation("Successfully exported template with {Count} lines", existingLines.Count);

            return new ExportStocktakingResultDto
            {
                FileContent = fileContent,
                FileName = fileName,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
        }
    }
}