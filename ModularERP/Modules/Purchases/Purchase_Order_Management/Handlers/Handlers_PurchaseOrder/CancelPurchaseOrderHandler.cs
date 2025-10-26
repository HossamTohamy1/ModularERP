using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;
using Serilog;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_PurchaseOrder
{
    public class CancelPurchaseOrderHandler : IRequestHandler<CancelPurchaseOrderCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<PurchaseOrder> _repository;
        private readonly ILogger<CancelPurchaseOrderHandler> _logger;

        public CancelPurchaseOrderHandler(IGeneralRepository<PurchaseOrder> repository,
            ILogger<CancelPurchaseOrderHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            CancelPurchaseOrderCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Cancelling purchase order with ID: {POId}", request.Id);

                var purchaseOrder = await _repository.GetByIDWithTracking(request.Id);
                if (purchaseOrder == null)
                {
                    throw new NotFoundException(
                        $"Purchase order with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                if (purchaseOrder.DocumentStatus == "Cancelled")
                {
                    throw new BusinessLogicException(
                        "Purchase order is already cancelled",
                        "PurchaseOrder");
                }

                if (purchaseOrder.DocumentStatus == "Closed")
                {
                    throw new BusinessLogicException(
                        "Cannot cancel a closed purchase order",
                        "PurchaseOrder");
                }

                // Validate no postings exist
                var hasGoodsReceipts = await _repository
                    .GetAll()
                    .Where(x => x.Id == request.Id)
                    .Select(x => x.GoodsReceipts.Any())
                    .FirstOrDefaultAsync(cancellationToken);

                if (hasGoodsReceipts)
                {
                    throw new BusinessLogicException(
                        "Cannot cancel purchase order with existing goods receipts. Please reverse receipts first.",
                        "PurchaseOrder");
                }

                var hasInvoices = await _repository
                    .GetAll()
                    .Where(x => x.Id == request.Id)
                    .Select(x => x.Invoices.Any())
                    .FirstOrDefaultAsync(cancellationToken);

                if (hasInvoices)
                {
                    throw new BusinessLogicException(
                        "Cannot cancel purchase order with existing invoices. Please void invoices first.",
                        "PurchaseOrder");
                }

                var hasPayments = purchaseOrder.DepositAmount > 0;
                if (hasPayments)
                {
                    throw new BusinessLogicException(
                        "Cannot cancel purchase order with deposits. Please refund deposits first.",
                        "PurchaseOrder");
                }

                purchaseOrder.DocumentStatus = "Cancelled";
                purchaseOrder.UpdatedAt = DateTime.UtcNow;
                purchaseOrder.Notes = string.IsNullOrEmpty(purchaseOrder.Notes)
                    ? $"Cancelled: {request.CancellationReason}"
                    : $"{purchaseOrder.Notes}\n\nCancelled: {request.CancellationReason}";

                await _repository.SaveChanges();

                _logger.LogInformation("Purchase order {PONumber} cancelled successfully", purchaseOrder.PONumber);

                return ResponseViewModel<bool>.Success(true, "Purchase order cancelled successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling purchase order with ID: {POId}", request.Id);
                throw;
            }
        }
    }
}
