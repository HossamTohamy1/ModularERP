using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;
using Serilog;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_PurchaseOrder
{
    public class RejectPurchaseOrderHandler : IRequestHandler<RejectPurchaseOrderCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<PurchaseOrder> _repository;
        private readonly ILogger<RejectPurchaseOrderHandler> _logger;

        public RejectPurchaseOrderHandler(IGeneralRepository<PurchaseOrder> repository,
            ILogger<RejectPurchaseOrderHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            RejectPurchaseOrderCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Rejecting purchase order with ID: {POId}", request.Id);

                var purchaseOrder = await _repository.GetByIDWithTracking(request.Id);
                if (purchaseOrder == null)
                {
                    throw new NotFoundException(
                        $"Purchase order with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                if (purchaseOrder.DocumentStatus != "Submitted")
                {
                    throw new BusinessLogicException(
                        "Only purchase orders in Submitted status can be rejected",
                        "PurchaseOrder");
                }

                // Return to Draft status
                purchaseOrder.DocumentStatus = "Draft";
                purchaseOrder.UpdatedAt = DateTime.UtcNow;
                purchaseOrder.Notes = string.IsNullOrEmpty(purchaseOrder.Notes)
                    ? $"Rejected: {request.RejectionReason}"
                    : $"{purchaseOrder.Notes}\n\nRejected: {request.RejectionReason}";

                await _repository.SaveChanges();

                _logger.LogInformation("Purchase order {PONumber} rejected and returned to Draft", purchaseOrder.PONumber);

                return ResponseViewModel<bool>.Success(true, "Purchase order rejected successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting purchase order with ID: {POId}", request.Id);
                throw;
            }
        }
    }

}
