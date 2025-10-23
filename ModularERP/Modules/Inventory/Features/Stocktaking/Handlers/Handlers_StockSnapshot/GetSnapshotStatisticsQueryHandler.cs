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
    public class GetSnapshotStatisticsQueryHandler : IRequestHandler<GetSnapshotStatisticsQuery, ResponseViewModel<SnapshotStatisticsDto>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly ILogger<GetSnapshotStatisticsQueryHandler> _logger;

        public GetSnapshotStatisticsQueryHandler(
            IGeneralRepository<StocktakingHeader> stocktakingRepository,
            IGeneralRepository<Product> productRepository,
            ILogger<GetSnapshotStatisticsQueryHandler> logger)
        {
            _stocktakingRepository = stocktakingRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<SnapshotStatisticsDto>> Handle(GetSnapshotStatisticsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Calculating snapshot statistics for stocktaking {StocktakingId}", request.StocktakingId);

                var stocktaking = await _stocktakingRepository
                    .GetAll()
                    .Where(s => s.Id == request.StocktakingId)
                    .Select(s => new
                    {
                        s.Id,
                        s.Number,
                        s.DateTime,
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
                    throw new NotFoundException(
                        $"Stocktaking with ID {request.StocktakingId} not found",
                        FinanceErrorCode.NotFound);
                }

                var productIds = stocktaking.Snapshots.Select(s => s.ProductId).ToList();

                var currentStocks = await _productRepository
                    .GetAll()
                    .Where(p => productIds.Contains(p.Id))
                    .Select(p => new
                    {
                        p.Id,
                        CurrentQty = p.InitialStock,
                        // Assuming cost exists in Product
                        UnitCost = p.PurchasePrice ?? 0
                    })
                    .ToListAsync(cancellationToken);

                var comparisons = stocktaking.Snapshots.Select(snapshot =>
                {
                    var currentStock = currentStocks.FirstOrDefault(cs => cs.Id == snapshot.ProductId);
                    var currentQty = currentStock?.CurrentQty ?? 0;
                    var unitCost = currentStock?.UnitCost ?? 0;
                    var drift = currentQty - snapshot.QtyAtStart;
                    var driftPercentage = snapshot.QtyAtStart != 0
                        ? Math.Abs((drift / snapshot.QtyAtStart) * 100)
                        : 0;

                    return new
                    {
                        ProductName = snapshot.ProductName,
                        SnapshotQty = snapshot.QtyAtStart,
                        CurrentQty = currentQty,
                        Drift = drift,
                        DriftPercentage = driftPercentage,
                        SnapshotValue = snapshot.QtyAtStart * unitCost,
                        CurrentValue = currentQty * unitCost,
                        DriftValue = drift * unitCost,
                        HasDrift = drift != 0
                    };
                }).ToList();

                var productsWithDrift = comparisons.Count(c => c.HasDrift);
                var productsExceedingThreshold = comparisons.Count(c => c.DriftPercentage > 5); // 5% default threshold
                var maxDrift = comparisons.MaxBy(c => c.DriftPercentage);

                var statistics = new SnapshotStatisticsDto
                {
                    StocktakingId = stocktaking.Id,
                    StocktakingNumber = stocktaking.Number,
                    SnapshotDate = stocktaking.DateTime,
                    TotalProducts = comparisons.Count,
                    ProductsWithDrift = productsWithDrift,
                    ProductsExceedingThreshold = productsExceedingThreshold,
                    TotalSnapshotValue = comparisons.Sum(c => c.SnapshotValue),
                    TotalCurrentValue = comparisons.Sum(c => c.CurrentValue),
                    TotalDriftValue = comparisons.Sum(c => Math.Abs(c.DriftValue)),
                    AverageDriftPercentage = comparisons.Any() ? comparisons.Average(c => c.DriftPercentage) : 0,
                    MaxDriftPercentage = maxDrift?.DriftPercentage ?? 0,
                    ProductWithMaxDrift = maxDrift?.ProductName ?? string.Empty
                };

                _logger.LogInformation("Successfully calculated statistics for stocktaking {StocktakingId}", request.StocktakingId);

                return ResponseViewModel<SnapshotStatisticsDto>.Success(statistics, "Statistics calculated successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating snapshot statistics");
                throw new BusinessLogicException(
                    "Failed to calculate snapshot statistics",
                    "Inventory",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
