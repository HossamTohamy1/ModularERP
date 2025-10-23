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
    public class GetProductSnapshotQueryHandler : IRequestHandler<GetProductSnapshotQuery, ResponseViewModel<StockSnapshotDto>>
    {
        private readonly IGeneralRepository<StockSnapshot> _snapshotRepository;
        private readonly ILogger<GetProductSnapshotQueryHandler> _logger;

        public GetProductSnapshotQueryHandler(
            IGeneralRepository<StockSnapshot> snapshotRepository,
            ILogger<GetProductSnapshotQueryHandler> logger)
        {
            _snapshotRepository = snapshotRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<StockSnapshotDto>> Handle(GetProductSnapshotQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching snapshot for product {ProductId} in stocktaking {StocktakingId}",
                    request.ProductId, request.StocktakingId);

                var snapshot = await _snapshotRepository
                    .GetAll()
                    .Where(s => s.StocktakingId == request.StocktakingId && s.ProductId == request.ProductId)
                    .Select(s => new StockSnapshotDto
                    {
                        SnapshotId = s.SnapshotId,
                        StocktakingId = s.StocktakingId,
                        ProductId = s.ProductId,
                        ProductName = s.Product.Name,
                        ProductSKU = s.Product.SKU,
                        QtyAtStart = s.QtyAtStart,
                        CreatedAt = s.CreatedAt
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (snapshot == null)
                {
                    _logger.LogWarning("Snapshot not found for product {ProductId} in stocktaking {StocktakingId}",
                        request.ProductId, request.StocktakingId);
                    throw new NotFoundException(
                        "Snapshot not found",
                        FinanceErrorCode.NotFound);
                }

                return ResponseViewModel<StockSnapshotDto>.Success(snapshot, "Snapshot retrieved successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product snapshot");
                throw new BusinessLogicException(
                    "Failed to fetch product snapshot",
                    "Inventory",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }

}
