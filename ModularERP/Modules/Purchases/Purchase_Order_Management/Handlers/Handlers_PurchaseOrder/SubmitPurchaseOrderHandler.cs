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
    public class SubmitPurchaseOrderHandler : IRequestHandler<SubmitPurchaseOrderCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<PurchaseOrder> _repository;
        private readonly ILogger<SubmitPurchaseOrderHandler> _logger;

        public SubmitPurchaseOrderHandler(IGeneralRepository<PurchaseOrder> repository,
            ILogger<SubmitPurchaseOrderHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            SubmitPurchaseOrderCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Submitting purchase order with ID: {POId}", request.Id);

                var purchaseOrder = await _repository.GetByIDWithTracking(request.Id);
                if (purchaseOrder == null)
                {
                    throw new NotFoundException(
                        $"Purchase order with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                if (purchaseOrder.DocumentStatus != DocumentStatus.Draft)
                {
                    throw new BusinessLogicException(
                        "Only purchase orders in Draft status can be submitted",
                        "PurchaseOrder");
                }

                purchaseOrder.DocumentStatus = DocumentStatus.Submitted;
                purchaseOrder.SubmittedBy = request.SubmittedBy;
                purchaseOrder.SubmittedAt = DateTime.UtcNow;
                purchaseOrder.UpdatedAt = DateTime.UtcNow;

                await _repository.SaveChanges();

                _logger.LogInformation("Purchase order {PONumber} submitted successfully", purchaseOrder.PONumber);

                return ResponseViewModel<bool>.Success(true, "Purchase order submitted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting purchase order with ID: {POId}", request.Id);
                throw;
            }
        }
    }
}
