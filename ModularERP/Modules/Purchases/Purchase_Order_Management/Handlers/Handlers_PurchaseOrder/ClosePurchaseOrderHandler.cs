using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Purchases_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;
using Serilog;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_PurchaseOrder
{
    public class ClosePurchaseOrderHandler : IRequestHandler<ClosePurchaseOrderCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<PurchaseOrder> _repository;
        private readonly ILogger<ClosePurchaseOrderHandler> _logger;

        public ClosePurchaseOrderHandler(IGeneralRepository<PurchaseOrder> repository,
            ILogger<ClosePurchaseOrderHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            ClosePurchaseOrderCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Closing purchase order with ID: {POId}", request.Id);

                var purchaseOrder = await _repository.GetByIDWithTracking(request.Id);
                if (purchaseOrder == null)
                {
                    throw new NotFoundException(
                        $"Purchase order with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                if (purchaseOrder.DocumentStatus == DocumentStatus.Closed)
                {
                    throw new BusinessLogicException(
                        "Purchase order is already closed",
                        "PurchaseOrder");
                }

                if (purchaseOrder.DocumentStatus != DocumentStatus.Approved)
                {
                    throw new BusinessLogicException(
                        "Only approved purchase orders can be closed",
                        "PurchaseOrder");
                }

                // Validate closure conditions
                if (purchaseOrder.ReceptionStatus != ReceptionStatus.FullyReceived && purchaseOrder.ReceptionStatus != ReceptionStatus.Returned)
                {
                    throw new BusinessLogicException(
                        "Cannot close purchase order. Reception status must be Fully Received or Returned.",
                        "PurchaseOrder");
                }

                if (purchaseOrder.PaymentStatus != PaymentStatus.PaidInFull && purchaseOrder.PaymentStatus != PaymentStatus.Refunded)
                {
                    throw new BusinessLogicException(
                        "Cannot close purchase order. Payment status must be Paid in Full or Refunded.",
                        "PurchaseOrder");
                }

                if (purchaseOrder.AmountDue != 0)
                {
                    throw new BusinessLogicException(
                        $"Cannot close purchase order with outstanding balance of {purchaseOrder.AmountDue} {purchaseOrder.CurrencyCode}",
                        "PurchaseOrder");
                }

                purchaseOrder.DocumentStatus = DocumentStatus.Closed;
                purchaseOrder.ClosedBy = request.ClosedBy;
                purchaseOrder.ClosedAt = DateTime.UtcNow;
                purchaseOrder.UpdatedAt = DateTime.UtcNow;

                await _repository.SaveChanges();

                _logger.LogInformation("Purchase order {PONumber} closed successfully", purchaseOrder.PONumber);

                return ResponseViewModel<bool>.Success(true, "Purchase order closed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing purchase order with ID: {POId}", request.Id);
                throw;
            }
        }
    }
}
