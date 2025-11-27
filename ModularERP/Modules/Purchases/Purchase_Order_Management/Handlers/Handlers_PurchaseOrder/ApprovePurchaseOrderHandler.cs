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
    public class ApprovePurchaseOrderHandler : IRequestHandler<ApprovePurchaseOrderCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<PurchaseOrder> _repository;
        private readonly ILogger<ApprovePurchaseOrderHandler> _logger;

        public ApprovePurchaseOrderHandler(IGeneralRepository<PurchaseOrder> repository,
            ILogger<ApprovePurchaseOrderHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            ApprovePurchaseOrderCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Approving purchase order with ID: {POId}", request.Id);

                var purchaseOrder = await _repository.GetByIDWithTracking(request.Id);
                if (purchaseOrder == null)
                {
                    throw new NotFoundException(
                        $"Purchase order with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                if (purchaseOrder.DocumentStatus != DocumentStatus.Submitted)
                {
                    throw new BusinessLogicException(
                        "Only purchase orders in Submitted status can be approved",
                        "PurchaseOrder");
                }

                purchaseOrder.DocumentStatus = DocumentStatus.Approved;
                purchaseOrder.ApprovedBy = request.ApprovedBy;
                purchaseOrder.ApprovedAt = DateTime.UtcNow;
                purchaseOrder.UpdatedAt = DateTime.UtcNow;

                await _repository.SaveChanges();

                _logger.LogInformation("Purchase order {PONumber} approved successfully", purchaseOrder.PONumber);

                return ResponseViewModel<bool>.Success(true, "Purchase order approved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving purchase order with ID: {POId}", request.Id);
                throw;
            }
        }
    }
}
