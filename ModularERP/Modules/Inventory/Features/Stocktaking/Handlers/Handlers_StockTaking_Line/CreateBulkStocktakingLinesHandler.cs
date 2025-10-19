using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Line;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Line;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTaking_Line
{
    public class CreateBulkStocktakingLinesHandler : IRequestHandler<CreateBulkStocktakingLinesCommand, ResponseViewModel<List<StocktakingLineDto>>>
    {
        private readonly IGeneralRepository<StocktakingLine> _lineRepository;
        private readonly IGeneralRepository<StocktakingHeader> _headerRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IGeneralRepository<WarehouseStock> _stockRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateBulkStocktakingLinesHandler> _logger;

        public CreateBulkStocktakingLinesHandler(
            IGeneralRepository<StocktakingLine> lineRepository,
            IGeneralRepository<StocktakingHeader> headerRepository,
            IGeneralRepository<Product> productRepository,
            IGeneralRepository<WarehouseStock> stockRepository,
            IMapper mapper,
            ILogger<CreateBulkStocktakingLinesHandler> logger)
        {
            _lineRepository = lineRepository;
            _headerRepository = headerRepository;
            _productRepository = productRepository;
            _stockRepository = stockRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<StocktakingLineDto>>> Handle(
            CreateBulkStocktakingLinesCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing CreateBulkStocktakingLinesCommand for {Count} lines",
                request.Lines.Count);

            // Validate stocktaking
            var stocktaking = await _headerRepository.GetByID(request.StocktakingId);
            if (stocktaking == null)
            {
                throw new NotFoundException(
                    $"Stocktaking with ID {request.StocktakingId} not found",
                    FinanceErrorCode.NotFound);
            }

            if (stocktaking.Status != StocktakingStatus.Draft &&
                stocktaking.Status != StocktakingStatus.Counting)
            {
                throw new BusinessLogicException(
                    $"Cannot add lines to stocktaking with status {stocktaking.Status}",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            // Get all product IDs
            var productIds = request.Lines.Select(l => l.ProductId).Distinct().ToList();

            // Validate all products exist
            var existingProducts = await _productRepository
                .Get(p => productIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            var missingProducts = productIds.Except(existingProducts).ToList();
            if (missingProducts.Any())
            {
                throw new NotFoundException(
                    $"Products not found: {string.Join(", ", missingProducts)}",
                    FinanceErrorCode.NotFound);
            }

            // Check for duplicates in request
            var duplicatesInRequest = request.Lines
                .GroupBy(l => l.ProductId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatesInRequest.Any())
            {
                throw new BusinessLogicException(
                    $"Duplicate products in request: {string.Join(", ", duplicatesInRequest)}",
                    "Inventory",
                    FinanceErrorCode.DuplicateEntity);
            }

            // Check for existing lines
            var existingLines = await _lineRepository
                .Get(l => l.StocktakingId == request.StocktakingId &&
                         productIds.Contains(l.ProductId))
                .Select(l => l.ProductId)
                .ToListAsync(cancellationToken);

            if (existingLines.Any())
            {
                throw new BusinessLogicException(
                    $"Some products already exist in stocktaking: {string.Join(", ", existingLines)}",
                    "Inventory",
                    FinanceErrorCode.DuplicateEntity);
            }

            // Get warehouse stocks for all products
            var warehouseStocks = await _stockRepository
                .Get(ws => ws.WarehouseId == stocktaking.WarehouseId &&
                          productIds.Contains(ws.ProductId))
                .ToDictionaryAsync(ws => ws.ProductId, cancellationToken);

            // Create lines
            var linesToAdd = new List<StocktakingLine>();
            foreach (var dto in request.Lines)
            {
                if (dto.PhysicalQty < 0)
                {
                    throw new BusinessLogicException(
                        $"Physical quantity cannot be negative for product {dto.ProductId}",
                        "Inventory",
                        FinanceErrorCode.ValidationError);
                }

                var systemQty = warehouseStocks.TryGetValue(dto.ProductId, out var stock)
                    ? stock.Quantity
                    : 0;

                var valuationCost = stock?.AverageUnitCost ?? 0;

                var line = new StocktakingLine
                {
                    Id = Guid.NewGuid(),
                    StocktakingId = request.StocktakingId,
                    ProductId = dto.ProductId,
                    UnitId = dto.UnitId,
                    PhysicalQty = dto.PhysicalQty,
                    SystemQtySnapshot = systemQty,
                    VarianceQty = dto.PhysicalQty - systemQty,
                    ValuationCost = valuationCost,
                    Note = dto.Note,
                    ImagePath = dto.ImagePath,
                    CreatedAt = DateTime.UtcNow
                };

                line.VarianceValue = line.VarianceQty * line.ValuationCost;
                linesToAdd.Add(line);
            }

            await _lineRepository.AddRangeAsync(linesToAdd);
            await _lineRepository.SaveChanges();

            // Update stocktaking status
            if (stocktaking.Status == StocktakingStatus.Draft)
            {
                stocktaking.Status = StocktakingStatus.Counting;
                await _headerRepository.Update(stocktaking);
            }

            _logger.LogInformation("Bulk created {Count} stocktaking lines", linesToAdd.Count);

            // Get created lines
            var lineIds = linesToAdd.Select(l => l.Id).ToList();
            var createdLines = await _lineRepository
                .Get(l => lineIds.Contains(l.Id))
                .ProjectTo<StocktakingLineDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return ResponseViewModel<List<StocktakingLineDto>>.Success(
                createdLines,
                $"Successfully created {createdLines.Count} stocktaking lines");
        }
    }
}