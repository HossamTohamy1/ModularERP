using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commend_InvocieItem;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_InvocieItem
{
    public class UpdateInvoiceItemHandler : IRequestHandler<UpdateInvoiceItemCommand, ResponseViewModel<InvoiceLineItemResponse>>
    {
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepository;
        private readonly IGeneralRepository<InvoiceLineItem> _lineItemRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateInvoiceItemHandler> _logger;

        public UpdateInvoiceItemHandler(
            IGeneralRepository<PurchaseInvoice> invoiceRepository,
            IGeneralRepository<InvoiceLineItem> lineItemRepository,
            IMapper mapper,
            ILogger<UpdateInvoiceItemHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _lineItemRepository = lineItemRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<InvoiceLineItemResponse>> Handle(UpdateInvoiceItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating invoice item {ItemId} for invoice {InvoiceId}",
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
                        "Cannot update items in fully paid invoices",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError
                    );
                }

                // Get existing line item
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

                // Store old values for recalculation
                decimal oldSubtotal = lineItem.Quantity * lineItem.UnitPrice;
                decimal oldTaxAmount = lineItem.TaxAmount;

                // Update line item
                lineItem.Description = request.Request.Description;
                lineItem.Quantity = request.Request.Quantity;
                lineItem.UnitPrice = request.Request.UnitPrice;
                lineItem.TaxAmount = request.Request.TaxAmount;
                lineItem.LineTotal = (request.Request.Quantity * request.Request.UnitPrice) + request.Request.TaxAmount;
                lineItem.UpdatedAt = DateTime.UtcNow;

                await _lineItemRepository.Update(lineItem);

                // Recalculate invoice totals
                decimal newSubtotal = lineItem.Quantity * lineItem.UnitPrice;
                decimal newTaxAmount = lineItem.TaxAmount;

                invoice.Subtotal = invoice.Subtotal - oldSubtotal + newSubtotal;
                invoice.TaxAmount = invoice.TaxAmount - oldTaxAmount + newTaxAmount;
                invoice.TotalAmount = invoice.Subtotal + invoice.TaxAmount;
                invoice.AmountDue = invoice.TotalAmount - invoice.DepositApplied;
                invoice.UpdatedAt = DateTime.UtcNow;

                await _invoiceRepository.Update(invoice);
                await _lineItemRepository.SaveChanges();

                _logger.LogInformation("Successfully updated invoice item {ItemId} for invoice {InvoiceId}",
                    request.ItemId, request.InvoiceId);

                var response = _mapper.Map<InvoiceLineItemResponse>(lineItem);

                return ResponseViewModel<InvoiceLineItemResponse>.Success(
                    response,
                    "Invoice item updated successfully"
                );
            }
            catch (BaseApplicationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating invoice item {ItemId} for invoice {InvoiceId}",
                    request.ItemId, request.InvoiceId);

                return ResponseViewModel<InvoiceLineItemResponse>.Error(
                    "An error occurred while updating the invoice item",
                    FinanceErrorCode.InternalServerError
                );
            }
        }
    }
}
