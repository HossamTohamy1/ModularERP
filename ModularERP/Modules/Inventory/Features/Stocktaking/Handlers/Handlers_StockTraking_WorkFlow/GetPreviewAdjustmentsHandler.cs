using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTraking_WorkFlow;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTraking_WorkFlow
{
    public class GetPreviewAdjustmentsHandler : IRequestHandler<GetPreviewAdjustmentsQuery, ResponseViewModel<PreviewAdjustmentsDto>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepo;
        private readonly IGeneralRepository<WarehouseStock> _warehouseStockRepo;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public GetPreviewAdjustmentsHandler(
            IGeneralRepository<StocktakingHeader> stocktakingRepo,
            IGeneralRepository<WarehouseStock> warehouseStockRepo,
            IMapper mapper,
            ILogger logger)
        {
            _stocktakingRepo = stocktakingRepo;
            _warehouseStockRepo = warehouseStockRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PreviewAdjustmentsDto>> Handle(
            GetPreviewAdjustmentsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.Information("Generating preview adjustments for stocktaking {StocktakingId}", request.StocktakingId);

                var stocktaking = await _stocktakingRepo.GetByID(request.StocktakingId);
                if (stocktaking == null)
                    throw new NotFoundException("Stocktaking not found", FinanceErrorCode.RecordNotFound);

                if (!stocktaking.UpdateSystem)
                {
                    return ResponseViewModel<PreviewAdjustmentsDto>.Success(
                        new PreviewAdjustmentsDto
                        {
                            StocktakingId = stocktaking.Id,
                            IsRecordOnly = true,
                            Adjustments = new List<AdjustmentPreviewDto>()
                        },
                        "Record-only mode: no adjustments will be posted");
                }

                var adjustments = new List<AdjustmentPreviewDto>();

                foreach (var line in stocktaking.Lines)
                {
                    var variance = line.PhysicalQty - line.SystemQtySnapshot;
                    if (variance == 0)
                        continue;

                    var warehouseStock = _warehouseStockRepo
                        .Get(ws => ws.WarehouseId == stocktaking.WarehouseId && ws.ProductId == line.ProductId)
                        .FirstOrDefault();

                    if (warehouseStock != null)
                    {
                        adjustments.Add(new AdjustmentPreviewDto
                        {
                            ProductId = line.ProductId,
                            ProductName = line.Product?.Name ?? "Unknown",
                            CurrentStock = warehouseStock.Quantity,
                            PhysicalCount = line.PhysicalQty,
                            Variance = variance,
                            AdjustmentType = variance > 0 ? "Write-On" : "Write-Off",
                            UnitCost = warehouseStock.AverageUnitCost ?? 0,
                            AdjustmentValue = variance * (warehouseStock.AverageUnitCost ?? 0)
                        });
                    }
                }

                var preview = new PreviewAdjustmentsDto
                {
                    StocktakingId = stocktaking.Id,
                    IsRecordOnly = false,
                    TotalAdjustments = adjustments.Count,
                    TotalWriteOffs = adjustments.Count(a => a.AdjustmentType == "Write-Off"),
                    TotalWriteOns = adjustments.Count(a => a.AdjustmentType == "Write-On"),
                    TotalValue = adjustments.Sum(a => Math.Abs(a.AdjustmentValue)),
                    Adjustments = adjustments
                };

                return ResponseViewModel<PreviewAdjustmentsDto>.Success(preview, "Preview generated successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error generating preview adjustments for stocktaking {StocktakingId}", request.StocktakingId);
                throw;
            }
        }
    }
}
