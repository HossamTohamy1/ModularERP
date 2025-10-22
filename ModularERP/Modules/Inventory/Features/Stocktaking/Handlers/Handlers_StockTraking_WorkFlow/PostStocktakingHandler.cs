using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commands_StockTraking_WorkFlow;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_WorkFlow;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Modules.Inventory.Features.StockTransactions.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTraking_WorkFlow
{
    public class PostStocktakingHandler : IRequestHandler<PostStocktakingCommand, ResponseViewModel<PostStocktakingDto>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepo;
        private readonly IGeneralRepository<StocktakingLine> _lineRepo;
        private readonly IGeneralRepository<WarehouseStock> _warehouseStockRepo;
        private readonly IGeneralRepository<StockTransaction> _transactionRepo;
        private readonly IGeneralRepository<ProductStats> _productStatsRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<PostStocktakingHandler> _logger;

        public PostStocktakingHandler(
            IGeneralRepository<StocktakingHeader> stocktakingRepo,
            IGeneralRepository<StocktakingLine> lineRepo,
            IGeneralRepository<WarehouseStock> warehouseStockRepo,
            IGeneralRepository<StockTransaction> transactionRepo,
            IGeneralRepository<ProductStats> productStatsRepo,
            IMapper mapper,
            ILogger<PostStocktakingHandler> logger)
        {
            _stocktakingRepo = stocktakingRepo;
            _lineRepo = lineRepo;
            _warehouseStockRepo = warehouseStockRepo;
            _transactionRepo = transactionRepo;
            _productStatsRepo = productStatsRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PostStocktakingDto>> Handle(
            PostStocktakingCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Posting stocktaking {StocktakingId}", request.StocktakingId);

                var stocktaking = await _stocktakingRepo.GetByID(request.StocktakingId);
                if (stocktaking == null)
                    throw new NotFoundException("Stocktaking not found", FinanceErrorCode.NotFound);

                if (stocktaking.Status != StocktakingStatus.Review && !request.ForcePost)
                    throw new BusinessLogicException("Stocktaking must be in Review status to post", "Inventory");

                if (!stocktaking.UpdateSystem)
                {
                    _logger.LogInformation("Stocktaking {StocktakingId} marked as record-only", request.StocktakingId);
                    stocktaking.Status = StocktakingStatus.Posted;
                    stocktaking.PostedBy = request.UserId;
                    stocktaking.PostedAt = DateTime.UtcNow;
                    await _stocktakingRepo.Update(stocktaking);

                    var resultDto = _mapper.Map<PostStocktakingDto>(stocktaking);
                    return ResponseViewModel<PostStocktakingDto>.Success(
                        resultDto,
                        "Stocktaking posted (no adjustments)");
                }

                // Get all lines for this stocktaking
                var lines = await _lineRepo
                    .Get(l => l.StocktakingId == request.StocktakingId)
                    .ToListAsync(cancellationToken);

                var adjustmentCount = 0;
                foreach (var line in lines)
                {
                    line.SystemQtyAtPost = line.SystemQtySnapshot;
                    line.VarianceQty = line.PhysicalQty - line.SystemQtyAtPost;

                    if (line.VarianceQty == 0)
                        continue;

                    adjustmentCount++;

                    var warehouseStock = await _warehouseStockRepo
                        .Get(ws => ws.WarehouseId == stocktaking.WarehouseId && ws.ProductId == line.ProductId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (warehouseStock != null)
                    {
                        decimal oldAuc = warehouseStock.AverageUnitCost ?? 0;
                        decimal variance = line.VarianceQty ?? 0;

                        if (variance > 0)
                        {
                            line.ValuationCost = oldAuc;
                            warehouseStock.Quantity += variance;
                            warehouseStock.AvailableQuantity = warehouseStock.Quantity - (warehouseStock.ReservedQuantity ?? 0);

                            _logger.LogInformation("Write-on: Product {ProductId} +{Qty}", line.ProductId, variance);
                        }
                        else
                        {
                            line.ValuationCost = oldAuc;
                            warehouseStock.Quantity += variance;
                            warehouseStock.AvailableQuantity = warehouseStock.Quantity - (warehouseStock.ReservedQuantity ?? 0);

                            _logger.LogInformation("Write-off: Product {ProductId} {Qty}", line.ProductId, variance);
                        }

                        line.VarianceValue = line.VarianceQty * line.ValuationCost;
                        await _lineRepo.Update(line);

                        // Update ProductStats
                        await UpdateProductStats(
                            line.ProductId,
                            stocktaking.CompanyId,
                            warehouseStock.Quantity,
                            warehouseStock.AverageUnitCost ?? 0,
                            cancellationToken);
                    }
                }

                await _warehouseStockRepo.SaveChanges();
                await _lineRepo.SaveChanges();

                stocktaking.Status = StocktakingStatus.Posted;
                stocktaking.PostedBy = request.UserId;
                stocktaking.PostedAt = DateTime.UtcNow;
                await _stocktakingRepo.Update(stocktaking);

                _logger.LogInformation("Stocktaking {StocktakingId} posted with {AdjustmentCount} adjustments",
                    request.StocktakingId, adjustmentCount);

                var result = _mapper.Map<PostStocktakingDto>(stocktaking);
                return ResponseViewModel<PostStocktakingDto>.Success(
                    result,
                    $"Stocktaking posted with {adjustmentCount} adjustments");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting stocktaking {StocktakingId}", request.StocktakingId);
                throw;
            }
        }

        private async Task UpdateProductStats(
            Guid productId,
            Guid companyId,
            decimal newQuantity,
            decimal newAvgCost,
            CancellationToken cancellationToken)
        {
            var productStats = await _productStatsRepo
                .Get(ps => ps.ProductId == productId && ps.CompanyId == companyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (productStats == null)
            {
                // Create new ProductStats if doesn't exist
                productStats = new ProductStats
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    CompanyId = companyId,
                    OnHandStock = newQuantity,
                    AvgUnitCost = newAvgCost,
                    LastUpdated = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                await _productStatsRepo.AddAsync(productStats);
                _logger.LogInformation("Created ProductStats for Product {ProductId}", productId);
            }
            else
            {
                // Update existing ProductStats
                productStats.OnHandStock = newQuantity;
                productStats.AvgUnitCost = newAvgCost;
                productStats.LastUpdated = DateTime.UtcNow;
                productStats.UpdatedAt = DateTime.UtcNow;

                await _productStatsRepo.Update(productStats);
                _logger.LogInformation("Updated ProductStats for Product {ProductId}: OnHand={OnHand}, AvgCost={AvgCost}",
                    productId, newQuantity, newAvgCost);
            }

            await _productStatsRepo.SaveChanges();
        }
    }
}