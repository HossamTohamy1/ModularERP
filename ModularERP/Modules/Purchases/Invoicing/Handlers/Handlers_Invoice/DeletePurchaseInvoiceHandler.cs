using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commands_Invoice;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_Invoice
{
    public class DeletePurchaseInvoiceHandler : IRequestHandler<DeletePurchaseInvoiceCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepository;
        private readonly IGeneralRepository<InvoiceLineItem> _lineItemRepository;
        private readonly ILogger<DeletePurchaseInvoiceHandler> _logger;

        public DeletePurchaseInvoiceHandler(
            IGeneralRepository<PurchaseInvoice> invoiceRepository,
            IGeneralRepository<InvoiceLineItem> lineItemRepository,
            ILogger<DeletePurchaseInvoiceHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _lineItemRepository = lineItemRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            DeletePurchaseInvoiceCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Starting deletion of purchase invoice: {InvoiceId}",
                    request.Id);

                var invoice = await _invoiceRepository.GetByID(request.Id);

                if (invoice == null)
                {
                    _logger.LogWarning("Purchase invoice not found: {InvoiceId}", request.Id);
                    throw new NotFoundException(
                        $"Purchase invoice with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                // Validate invoice can be deleted
                if (invoice.PaymentStatus == "PaidInFull")
                {
                    _logger.LogWarning(
                        "Cannot delete invoice {InvoiceId} - already paid in full",
                        request.Id);
                    throw new BusinessLogicException(
                        "Cannot delete invoice that is paid in full. Please create a refund instead.",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Check if there are any payments
                var hasPayments = _invoiceRepository
                    .Get(i => i.Id == request.Id)
                    .Any(i => i.Payments.Any());

                if (hasPayments)
                {
                    _logger.LogWarning(
                        "Cannot delete invoice {InvoiceId} - has associated payments",
                        request.Id);
                    throw new BusinessLogicException(
                        "Cannot delete invoice with associated payments. Please reverse payments first.",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Soft delete line items
                var lineItems = _lineItemRepository
                    .Get(li => li.InvoiceId == request.Id)
                    .ToList();

                foreach (var lineItem in lineItems)
                {
                    await _lineItemRepository.Delete(lineItem.Id);
                }

                await _lineItemRepository.SaveChanges();

                _logger.LogInformation(
                    "Deleted {Count} line items for invoice: {InvoiceId}",
                    lineItems.Count, request.Id);

                // Soft delete invoice
                await _invoiceRepository.Delete(request.Id);
                await _invoiceRepository.SaveChanges();

                _logger.LogInformation(
                    "Purchase invoice deleted successfully: {InvoiceId}",
                    request.Id);

                return ResponseViewModel<bool>.Success(
                    true,
                    "Purchase invoice deleted successfully");
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
                _logger.LogError(ex,
                    "Error deleting purchase invoice: {InvoiceId}",
                    request.Id);
                throw new BusinessLogicException(
                    "Failed to delete purchase invoice",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}