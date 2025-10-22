using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockSnapshot;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockSnapshot;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockSnapshot
{
    public class CompareSnapshotQueryHandler : IRequestHandler<CompareSnapshotQuery, ResponseViewModel<List<SnapshotComparisonDto>>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly ILogger<CompareSnapshotQueryHandler> _logger;

        public CompareSnapshotQueryHandler(
            IGeneralRepository<StocktakingHeader> stocktakingRepository,
            IGeneralRepository<Product> productRepository,
            ILogger<CompareSnapshotQueryHandler> logger)
        {
            _stocktakingRepository = stocktakingRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<SnapshotComparisonDto>>> Handle(CompareSnapshotQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Comparing snapshot for stocktaking {StocktakingId} with threshold {Threshold}%",
                    request.StocktakingId, request.DriftThreshold);

                var stocktaking = await _stocktakingRepository
                    .GetAll()
                    .Where(s => s.Id == request.StocktakingId)
                    .Select(s => new
                    {
                        s.Id,
                        s.WarehouseId,
                        Snapshots = s.Snapshots.Select(snap => new
                        {
                            snap.ProductId,
                            snap.QtyAtStart,
                            ProductName = snap.Product.Name,
                            ProductSKU = snap.Product.SKU
                        }).ToList()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (stocktaking == null)
                {
                    _logger.LogWarning("Stocktaking {StocktakingId} not found", request.StocktakingId);
                    throw new NotFoundException(
                        $"Stocktaking with ID {request.StocktakingId} not found",
                        FinanceErrorCode.NotFound);
                }

                var productIds = stocktaking.Snapshots.Select(s => s.ProductId).ToList();

                // Get current stock levels (assuming you have WarehouseStock table)
                // For now, using placeholder - adjust based on your actual stock tracking
                var currentStocks = await _productRepository
                    .GetAll()
                    .Where(p => productIds.Contains(p.Id))
                    .Select(p => new
                    {
                        p.Id,
                        CurrentQty = p.InitialStock // Replace with actual stock level from WarehouseStock
                    })
                    .ToListAsync(cancellationToken);

                var comparisons = stocktaking.Snapshots.Select(snapshot =>
                {
                    var currentStock = currentStocks.FirstOrDefault(cs => cs.Id == snapshot.ProductId);
                    var currentQty = currentStock?.CurrentQty ?? 0;
                    var drift = currentQty - snapshot.QtyAtStart;
                    var driftPercentage = snapshot.QtyAtStart != 0
                        ? Math.Abs((drift / snapshot.QtyAtStart) * 100)
                        : 0;

                    return new SnapshotComparisonDto
                    {
                        ProductId = snapshot.ProductId,
                        ProductName = snapshot.ProductName,
                        ProductSKU = snapshot.ProductSKU,
                        SnapshotQty = snapshot.QtyAtStart,
                        CurrentQty = currentQty,
                        Drift = drift,
                        DriftPercentage = driftPercentage,
                        ExceedsThreshold = driftPercentage > request.DriftThreshold
                    };
                }).ToList();

                var exceedingCount = comparisons.Count(c => c.ExceedsThreshold);

                _logger.LogInformation(
                    "Snapshot comparison completed. {ExceedingCount} of {TotalCount} products exceed {Threshold}% threshold",
                    exceedingCount, comparisons.Count, request.DriftThreshold);

                return ResponseViewModel<List<SnapshotComparisonDto>>.Success(
                    comparisons,
                    $"Comparison completed. {exceedingCount} products exceed threshold");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing snapshot for stocktaking {StocktakingId}", request.StocktakingId);
                throw new BusinessLogicException(
                    "Failed to compare snapshot",
                    "Inventory",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
