using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTaking_Header
{
    public class DeleteStocktakingHandler : IRequestHandler<DeleteStocktakingCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _repo;
        private readonly ILogger<DeleteStocktakingHandler> _logger;

        public DeleteStocktakingHandler(
            IGeneralRepository<StocktakingHeader> repo,
            ILogger<DeleteStocktakingHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            DeleteStocktakingCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting stocktaking with ID {StocktakingId}", request.Id);

                var stocktaking = await _repo.GetByIDWithTracking(request.Id);

                if (stocktaking == null)
                {
                    _logger.LogWarning("Stocktaking with ID {StocktakingId} not found", request.Id);
                    throw new NotFoundException(
                        $"Stocktaking with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                if (stocktaking.Status != StocktakingStatus.Draft)
                {
                    _logger.LogWarning("Cannot delete stocktaking {StocktakingId} with status {Status}",
                        request.Id, stocktaking.Status);
                    throw new BusinessLogicException(
                        "Only Draft stocktakings can be deleted",
                        "Inventory",
                        FinanceErrorCode.BusinessLogicError);
                }

                await _repo.Delete(request.Id);

                _logger.LogInformation("Stocktaking {StocktakingId} deleted successfully", request.Id);

                return ResponseViewModel<bool>.Success(
                    true,
                    "Stocktaking deleted successfully");
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
                _logger.LogError(ex, "Error deleting stocktaking with ID {StocktakingId}", request.Id);
                throw new BusinessLogicException(
                    "An error occurred while deleting stocktaking",
                    ex,
                    "Inventory");
            }
        }
    }
}