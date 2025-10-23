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
    public class GetSnapshotExceptionsQueryHandler : IRequestHandler<GetSnapshotExceptionsQuery, ResponseViewModel<List<SnapshotComparisonDto>>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly ILogger<GetSnapshotExceptionsQueryHandler> _logger;

        public GetSnapshotExceptionsQueryHandler(
            IGeneralRepository<StocktakingHeader> stocktakingRepository,
            IGeneralRepository<Product> productRepository,
            ILogger<GetSnapshotExceptionsQueryHandler> logger)
        {
            _stocktakingRepository = stocktakingRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<SnapshotComparisonDto>>> Handle(GetSnapshotExceptionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching snapshot exceptions for stocktaking {StocktakingId} with threshold {Threshold}%",
                    request.StocktakingId, request.DriftThreshold);

                var stocktaking = await _stocktakingRepository
                    .GetAll()
                    .Where(s => s.Id == request.StocktakingId)
                    .Select(s => new
                    {
                        s.Id,
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
                        CurrentQty = p.InitialStock
                    })
                    .ToListAsync(cancellationToken);

                var exceptions = stocktaking.Snapshots
                    .Select(snapshot =>
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
                    })
                    .Where(c => c.ExceedsThreshold)
                    .OrderByDescending(c => c.DriftPercentage)
                    .ToList();

                _logger.LogInformation("Found {Count} products exceeding {Threshold}% threshold",
                    exceptions.Count, request.DriftThreshold);

                return ResponseViewModel<List<SnapshotComparisonDto>>.Success(
                    exceptions,
                    $"Found {exceptions.Count} products exceeding threshold");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching snapshot exceptions");
                throw new BusinessLogicException(
                    "Failed to fetch snapshot exceptions",
                    "Inventory",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }

}
