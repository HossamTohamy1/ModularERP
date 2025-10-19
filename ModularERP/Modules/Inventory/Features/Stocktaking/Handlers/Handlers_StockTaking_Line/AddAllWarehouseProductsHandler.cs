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
    public class AddAllWarehouseProductsHandler : IRequestHandler<AddAllWarehouseProductsCommand, ResponseViewModel<List<StocktakingLineDto>>>
    {
        private readonly IGeneralRepository<StocktakingLine> _lineRepository;
        private readonly IGeneralRepository<StocktakingHeader> _headerRepository;
        private readonly IGeneralRepository<WarehouseStock> _stockRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AddAllWarehouseProductsHandler> _logger;

        public AddAllWarehouseProductsHandler(
            IGeneralRepository<StocktakingLine> lineRepository,
            IGeneralRepository<StocktakingHeader> headerRepository,
            IGeneralRepository<WarehouseStock> stockRepository,
            IMapper mapper,
            ILogger<AddAllWarehouseProductsHandler> logger)
        {
            _lineRepository = lineRepository;
            _headerRepository = headerRepository;
            _stockRepository = stockRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<StocktakingLineDto>>> Handle(
            AddAllWarehouseProductsCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing AddAllWarehouseProductsCommand for stocktaking {StocktakingId}",
                request.StocktakingId);

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

            // Get existing lines
            var existingProductIds = await _lineRepository
                .Get(l => l.StocktakingId == request.StocktakingId)
                .Select(l => l.ProductId)
                .ToListAsync(cancellationToken);

            // Get all warehouse stocks excluding existing
            var warehouseStocks = await _stockRepository
                .Get(ws => ws.WarehouseId == stocktaking.WarehouseId &&
                          ws.Quantity > 0 &&
                          !existingProductIds.Contains(ws.ProductId))
                .ToListAsync(cancellationToken);

            if (!warehouseStocks.Any())
            {
                _logger.LogInformation("No new products to add for stocktaking {StocktakingId}",
                    request.StocktakingId);
                return ResponseViewModel<List<StocktakingLineDto>>.Success(
                    new List<StocktakingLineDto>(),
                    "No new products found to add");
            }

            // Create lines for all products
            var linesToAdd = new List<StocktakingLine>();
            foreach (var stock in warehouseStocks)
            {
                var line = new StocktakingLine
                {
                    Id = Guid.NewGuid(),
                    StocktakingId = request.StocktakingId,
                    ProductId = stock.ProductId,
                    PhysicalQty = 0, // User will fill this
                    SystemQtySnapshot = stock.Quantity,
                    VarianceQty = -stock.Quantity, // Negative until user enters count
                    ValuationCost = stock.AverageUnitCost ?? 0,
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

            _logger.LogInformation("Added {Count} warehouse products to stocktaking {StocktakingId}",
                linesToAdd.Count, request.StocktakingId);

            // Get created lines
            var lineIds = linesToAdd.Select(l => l.Id).ToList();
            var createdLines = await _lineRepository
                .Get(l => lineIds.Contains(l.Id))
                .ProjectTo<StocktakingLineDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return ResponseViewModel<List<StocktakingLineDto>>.Success(
                createdLines,
                $"Successfully added {createdLines.Count} warehouse products");
        }
    }
}