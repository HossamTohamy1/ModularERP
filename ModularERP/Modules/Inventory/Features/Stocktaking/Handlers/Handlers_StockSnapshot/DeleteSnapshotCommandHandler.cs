using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockSnapshot;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockSnapshot
{
    public class DeleteSnapshotCommandHandler : IRequestHandler<DeleteSnapshotCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepository;
        private readonly IGeneralRepository<StockSnapshot> _snapshotRepository;
        private readonly ILogger<DeleteSnapshotCommandHandler> _logger;

        public DeleteSnapshotCommandHandler(
            IGeneralRepository<StocktakingHeader> stocktakingRepository,
            IGeneralRepository<StockSnapshot> snapshotRepository,
            ILogger<DeleteSnapshotCommandHandler> logger)
        {
            _stocktakingRepository = stocktakingRepository;
            _snapshotRepository = snapshotRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteSnapshotCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting snapshot for stocktaking {StocktakingId}", request.StocktakingId);

                var stocktaking = await _stocktakingRepository
                    .GetAll()
                    .Where(s => s.Id == request.StocktakingId)
                    .Select(s => new
                    {
                        s.Id,
                        s.Status
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (stocktaking == null)
                {
                    throw new NotFoundException(
                        $"Stocktaking with ID {request.StocktakingId} not found",
                        FinanceErrorCode.NotFound);
                }

                // Validate status - only Draft can have snapshot deleted
                if (stocktaking.Status != StocktakingStatus.Draft)
                {
                    _logger.LogWarning("Cannot delete snapshot for stocktaking {StocktakingId} with status {Status}",
                        request.StocktakingId, stocktaking.Status);
                    throw new BusinessLogicException(
                        $"Cannot delete snapshot. Stocktaking is in {stocktaking.Status} status",
                        "Inventory",
                        FinanceErrorCode.BusinessLogicError);
                }

                var snapshots = await _snapshotRepository
                    .GetAll()
                    .Where(s => s.StocktakingId == request.StocktakingId)
                    .ToListAsync(cancellationToken);

                if (!snapshots.Any())
                {
                    _logger.LogWarning("No snapshots found for stocktaking {StocktakingId}", request.StocktakingId);
                    throw new NotFoundException(
                        "No snapshots found for this stocktaking",
                        FinanceErrorCode.NotFound);
                }

                foreach (var snapshot in snapshots)
                {
                    await _snapshotRepository.Delete(snapshot.Id);
                }

                _logger.LogInformation("Successfully deleted {Count} snapshots for stocktaking {StocktakingId}",
                    snapshots.Count, request.StocktakingId);

                return ResponseViewModel<bool>.Success(true,
                    $"Successfully deleted {snapshots.Count} snapshots");
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
                _logger.LogError(ex, "Error deleting snapshot for stocktaking {StocktakingId}", request.StocktakingId);
                throw new BusinessLogicException(
                    "Failed to delete snapshot",
                    "Inventory",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
