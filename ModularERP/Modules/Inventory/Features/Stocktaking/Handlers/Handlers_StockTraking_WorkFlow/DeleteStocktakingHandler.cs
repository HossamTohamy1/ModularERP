using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commands_StockTraking_WorkFlow;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTraking_WorkFlow
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
                _logger.LogInformation("Deleting stocktaking {StocktakingId}", request.StocktakingId);

                var entity = await _repo.GetByID(request.StocktakingId);
                if (entity == null)
                    throw new NotFoundException("Stocktaking not found", FinanceErrorCode.NotFound);

                if (entity.Status != StocktakingStatus.Draft)
                    throw new BusinessLogicException("Only draft stocktakings can be deleted", "Inventory");

                await _repo.Delete(request.StocktakingId);

                return ResponseViewModel<bool>.Success(true, "Stocktaking deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting stocktaking {StocktakingId}", request.StocktakingId);
                throw;
            }
        }
    }
}