using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRNItem;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Handlers.Handlers_GRNItem
{
    public class DeleteGRNItemHandler : IRequestHandler<DeleteGRNItemCommand, Unit>
    {
        private readonly IGeneralRepository<GRNLineItem> _grnLineItemRepository;
        private readonly ILogger<DeleteGRNItemHandler> _logger;

        public DeleteGRNItemHandler(
            IGeneralRepository<GRNLineItem> grnLineItemRepository,
            ILogger<DeleteGRNItemHandler> logger)
        {
            _grnLineItemRepository = grnLineItemRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(DeleteGRNItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting GRN item {ItemId} from GRN {GRNId}", request.ItemId, request.GRNId);

                var lineItem = await _grnLineItemRepository.GetByID(request.ItemId);
                if (lineItem == null || lineItem.GRNId != request.GRNId)
                {
                    throw new NotFoundException(
                        $"GRN line item with ID {request.ItemId} not found in GRN {request.GRNId}",
                        FinanceErrorCode.NotFound);
                }

                await _grnLineItemRepository.Delete(request.ItemId);

                _logger.LogInformation("GRN item {ItemId} deleted successfully", request.ItemId);

                return Unit.Value;
            }
            catch (Exception ex) when (ex is not BaseApplicationException)
            {
                _logger.LogError(ex, "Error deleting GRN item {ItemId}", request.ItemId);
                throw new BusinessLogicException(
                    $"Error deleting GRN item: {ex.Message}",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}