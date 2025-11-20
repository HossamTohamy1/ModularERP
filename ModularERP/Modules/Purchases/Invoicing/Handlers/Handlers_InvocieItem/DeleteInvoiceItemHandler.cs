using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commend_InvocieItem;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_InvocieItem
{
    public class DeleteInvoiceItemHandler : IRequestHandler<DeleteInvoiceItemCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepository;
        private readonly IGeneralRepository<InvoiceLineItem> _lineItemRepository;
        private readonly ILogger<DeleteInvoiceItemHandler> _logger;

        public DeleteInvoiceItemHandler(
            IGeneralRepository<PurchaseInvoice> invoiceRepository,
            IGeneralRepository<InvoiceLineItem> lineItemRepository,
            ILogger<DeleteInvoiceItemHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _lineItemRepository = lineItemRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteInvoiceItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting invoice item {ItemId} from invoice {InvoiceId}",
                    request.ItemId, request.InvoiceId);

                // Validate invoice exists
                var invoice = await _invoiceRepository.GetByID(request.InvoiceId);
                if (invoice == null)
                {
                    _logger.LogWarning("Invoice not found with ID: {InvoiceId}", request.InvoiceId);
                    throw new NotFoundException(
                        $"Invoice with ID {request.InvoiceId} not found",
                        FinanceErrorCode.NotFound
                    );
                }

                if (invoice.PaymentStatus == "PaidInFull")
                {
                    throw new BusinessLogicException(
                        "Cannot delete items from fully paid invoices",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError
                    );
                }

                // Get line item
                var lineItem = await _lineItemRepository
                    .GetAll()
                    .FirstOrDefaultAsync(li => li.Id == request.ItemId && li.InvoiceId == request.InvoiceId, cancellationToken);

                if (lineItem == null)
                {
                    _logger.LogWarning("Invoice item not found with ID: {ItemId} for invoice {InvoiceId}",
                        request.ItemId, request.InvoiceId);
                    throw new NotFoundException(
                        $"Invoice item with ID {request.ItemId} not found",
                        FinanceErrorCode.NotFound
                    );
                }

                // Check if it's the last item
                var itemCount = await _lineItemRepository
                    .GetAll()
                    .CountAsync(li => li.InvoiceId == request.InvoiceId, cancellationToken);

                if (itemCount == 1)
                {
                    throw new BusinessLogicException(
                        "Cannot delete the last item from an invoice",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError
                    );
                }

                // Update invoice totals
                decimal subtotalToRemove = lineItem.Quantity * lineItem.UnitPrice;
                invoice.Subtotal -= subtotalToRemove;
                invoice.TaxAmount -= lineItem.TaxAmount;
                invoice.TotalAmount = invoice.Subtotal + invoice.TaxAmount;
                invoice.AmountDue = invoice.TotalAmount - invoice.DepositApplied;
                invoice.UpdatedAt = DateTime.UtcNow;

                // Soft delete the line item
                await _lineItemRepository.Delete(request.ItemId);
                await _invoiceRepository.Update(invoice);
                await _lineItemRepository.SaveChanges();

                _logger.LogInformation("Successfully deleted invoice item {ItemId} from invoice {InvoiceId}",
                    request.ItemId, request.InvoiceId);

                return ResponseViewModel<bool>.Success(
                    true,
                    "Invoice item deleted successfully"
                );
            }
            catch (BaseApplicationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting invoice item {ItemId} from invoice {InvoiceId}",
                    request.ItemId, request.InvoiceId);

                return ResponseViewModel<bool>.Error(
                    "An error occurred while deleting the invoice item",
                    FinanceErrorCode.InternalServerError
                );
            }
        }
    }
}