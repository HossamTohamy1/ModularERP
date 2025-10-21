using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commands_StockTraking_WorkFlow;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_WorkFlow;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Modules.Inventory.Features.StockTransactions.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTraking_WorkFlow
{
    public class PostStocktakingHandler : IRequestHandler<PostStocktakingCommand, ResponseViewModel<PostStocktakingDto>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepo;
        private readonly IGeneralRepository<WarehouseStock> _warehouseStockRepo;
        private readonly IGeneralRepository<StockTransaction> _transactionRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<PostStocktakingHandler> _logger;

        public PostStocktakingHandler(
            IGeneralRepository<StocktakingHeader> stocktakingRepo,
            IGeneralRepository<WarehouseStock> warehouseStockRepo,
            IGeneralRepository<StockTransaction> transactionRepo,
            IMapper mapper,
            ILogger<PostStocktakingHandler> logger)
        {
            _stocktakingRepo = stocktakingRepo;
            _warehouseStockRepo = warehouseStockRepo;
            _transactionRepo = transactionRepo;
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
                    return ResponseViewModel<PostStocktakingDto>.Success(
                        _mapper.Map<PostStocktakingDto>(stocktaking),
                        "Stocktaking posted (no adjustments)");
                }

                var adjustmentCount = 0;
                foreach (var line in stocktaking.Lines)
                {
                    line.SystemQtyAtPost = line.SystemQtySnapshot;
                    line.VarianceQty = line.PhysicalQty - line.SystemQtyAtPost;

                    if (line.VarianceQty == 0)
                        continue;

                    var warehouseStock = _warehouseStockRepo
                        .Get(ws => ws.WarehouseId == stocktaking.WarehouseId && ws.ProductId == line.ProductId)
                        .FirstOrDefault();

                    if (warehouseStock != null)
                    {
                        decimal oldAuc = warehouseStock.AverageUnitCost ?? 0;
                        decimal oldQty = warehouseStock.Quantity;
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
                    }
                }

                await _stocktakingRepo.SaveChanges();
                await _transactionRepo.SaveChanges();

                stocktaking.Status = StocktakingStatus.Posted;
                stocktaking.PostedBy = request.UserId;
                stocktaking.PostedAt = DateTime.UtcNow;
                await _stocktakingRepo.Update(stocktaking);

                _logger.LogInformation("Stocktaking {StocktakingId} posted with {AdjustmentCount} adjustments",
                    request.StocktakingId, adjustmentCount);

                return ResponseViewModel<PostStocktakingDto>.Success(
                    _mapper.Map<PostStocktakingDto>(stocktaking),
                    $"Stocktaking posted with {adjustmentCount} adjustments");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting stocktaking {StocktakingId}", request.StocktakingId);
                throw;
            }
        }
    }
}
