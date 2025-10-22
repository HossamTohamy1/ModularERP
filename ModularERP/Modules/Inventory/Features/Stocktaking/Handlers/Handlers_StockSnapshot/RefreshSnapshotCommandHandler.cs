using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockSnapshot;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockSnapshot;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockSnapshot
{
    public class RefreshSnapshotCommandHandler : IRequestHandler<RefreshSnapshotCommand, ResponseViewModel<RefreshSnapshotDto>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepository;
        private readonly IGeneralRepository<StockSnapshot> _snapshotRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RefreshSnapshotCommandHandler> _logger;

        public RefreshSnapshotCommandHandler(
            IGeneralRepository<StocktakingHeader> stocktakingRepository,
            IGeneralRepository<StockSnapshot> snapshotRepository,
            IGeneralRepository<Product> productRepository,
            IMapper mapper,
            ILogger<RefreshSnapshotCommandHandler> logger)
        {
            _stocktakingRepository = stocktakingRepository;
            _snapshotRepository = snapshotRepository;
            _productRepository = productRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<RefreshSnapshotDto>> Handle(RefreshSnapshotCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Refreshing snapshot for stocktaking {StocktakingId}", request.StocktakingId);

                var stocktaking = await _stocktakingRepository
                    .GetAll()
                    .Where(s => s.Id == request.StocktakingId)
                    .Select(s => new
                    {
                        s.Id,
                        s.Status,
                        s.WarehouseId
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (stocktaking == null)
                {
                    _logger.LogWarning("Stocktaking {StocktakingId} not found", request.StocktakingId);
                    throw new NotFoundException(
                        $"Stocktaking with ID {request.StocktakingId} not found",
                        FinanceErrorCode.NotFound);
                }

                // Validate status - only Draft or Counting can be refreshed
                if (stocktaking.Status != StocktakingStatus.Draft &&
                    stocktaking.Status != StocktakingStatus.Counting)
                {
                    _logger.LogWarning("Cannot refresh snapshot for stocktaking {StocktakingId} with status {Status}",
                        request.StocktakingId, stocktaking.Status);
                    throw new BusinessLogicException(
                        $"Cannot refresh snapshot. Stocktaking is in {stocktaking.Status} status",
                        "Inventory",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Get existing snapshots
                var existingSnapshots = await _snapshotRepository
                    .GetAll()
                    .Where(s => s.StocktakingId == request.StocktakingId)
                    .ToListAsync(cancellationToken);

                var productIds = existingSnapshots.Select(s => s.ProductId).ToList();

                // Get current stock levels
                var currentStocks = await _productRepository
                    .GetAll()
                    .Where(p => productIds.Contains(p.Id))
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.SKU,
                        CurrentQty = p.InitialStock // Replace with actual stock level
                    })
                    .ToListAsync(cancellationToken);

                var refreshedSnapshots = new List<StockSnapshotDto>();

                // Update snapshots with current quantities
                foreach (var snapshot in existingSnapshots)
                {
                    var currentStock = currentStocks.FirstOrDefault(cs => cs.Id == snapshot.ProductId);
                    if (currentStock != null)
                    {
                        snapshot.QtyAtStart = (decimal)currentStock.CurrentQty;
                        snapshot.CreatedAt = DateTime.UtcNow;

                        refreshedSnapshots.Add(new StockSnapshotDto
                        {
                            SnapshotId = snapshot.SnapshotId,
                            StocktakingId = snapshot.StocktakingId,
                            ProductId = snapshot.ProductId,
                            ProductName = currentStock.Name,
                            ProductSKU = currentStock.SKU,
                            QtyAtStart = snapshot.QtyAtStart,
                            CreatedAt = snapshot.CreatedAt
                        });
                    }
                }

                await _snapshotRepository.SaveChanges();

                _logger.LogInformation("Successfully refreshed {Count} snapshots for stocktaking {StocktakingId}",
                    refreshedSnapshots.Count, request.StocktakingId);

                var result = new RefreshSnapshotDto
                {
                    StocktakingId = request.StocktakingId,
                    ProductsRefreshed = refreshedSnapshots.Count,
                    RefreshedAt = DateTime.UtcNow,
                    UpdatedSnapshots = refreshedSnapshots
                };

                return ResponseViewModel<RefreshSnapshotDto>.Success(
                    result,
                    $"Successfully refreshed {refreshedSnapshots.Count} product snapshots");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (BusinessLogicException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing snapshot for stocktaking {StocktakingId}", request.StocktakingId);
                throw new BusinessLogicException(
                    "Failed to refresh snapshot",
                    "Inventory",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}