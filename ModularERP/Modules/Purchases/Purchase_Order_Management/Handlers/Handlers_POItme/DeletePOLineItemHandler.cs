using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_POItme;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;
using Serilog;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_POItme
{
    public class DeletePOLineItemHandler : IRequestHandler<DeletePOLineItemCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<POLineItem> _lineItemRepository;
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly ILogger<DeletePOLineItemHandler> _logger;

        public DeletePOLineItemHandler(
            IGeneralRepository<POLineItem> lineItemRepository,
            IGeneralRepository<PurchaseOrder> poRepository,
            ILogger<DeletePOLineItemHandler> logger)
        {
            _lineItemRepository = lineItemRepository;
            _poRepository = poRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            DeletePOLineItemCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting line item {LineItemId} from Purchase Order {PurchaseOrderId}",
                    request.LineItemId, request.PurchaseOrderId);

                // Verify PO exists and is in valid state
                var po = await _poRepository.GetByID(request.PurchaseOrderId);
                if (po == null)
                {
                    throw new NotFoundException(
                        $"Purchase Order with ID {request.PurchaseOrderId} not found",
                        FinanceErrorCode.NotFound);
                }

                // Check if PO is in Draft or Submitted status (can delete items)
                if (po.DocumentStatus != "Draft" && po.DocumentStatus != "Submitted")
                {
                    throw new BusinessLogicException(
                        $"Cannot delete items from Purchase Order in {po.DocumentStatus} status",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Get line item
                var lineItem = await _lineItemRepository.GetByID(request.LineItemId);
                if (lineItem == null)
                {
                    throw new NotFoundException(
                        $"Line item with ID {request.LineItemId} not found",
                        FinanceErrorCode.NotFound);
                }

                // Verify line item belongs to the specified PO
                if (lineItem.PurchaseOrderId != request.PurchaseOrderId)
                {
                    throw new BusinessLogicException(
                        "Line item does not belong to the specified Purchase Order",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Check if item has been received (prevent deletion)
                if (lineItem.ReceivedQuantity > 0)
                {
                    throw new BusinessLogicException(
                        "Cannot delete line item that has already been partially or fully received",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Soft delete
                await _lineItemRepository.Delete(request.LineItemId);

                _logger.LogInformation("Line item {LineItemId} deleted successfully", request.LineItemId);

                return ResponseViewModel<bool>.Success(true, "Line item deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting line item {LineItemId}", request.LineItemId);
                throw;
            }
        }
    }
}
