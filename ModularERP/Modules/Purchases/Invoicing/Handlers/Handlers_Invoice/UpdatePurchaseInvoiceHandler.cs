using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commands_Invoice;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_Invoice;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Modules.Purchases.Invoicing.Services;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_Invoice
{
    public class UpdatePurchaseInvoiceHandler : IRequestHandler<UpdatePurchaseInvoiceCommand, ResponseViewModel<PurchaseInvoiceDto>>
    {
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepository;
        private readonly IGeneralRepository<InvoiceLineItem> _lineItemRepository;
        private readonly IPurchaseInvoiceService _invoiceService;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdatePurchaseInvoiceHandler> _logger;

        public UpdatePurchaseInvoiceHandler(
            IGeneralRepository<PurchaseInvoice> invoiceRepository,
            IGeneralRepository<InvoiceLineItem> lineItemRepository,
            IPurchaseInvoiceService invoiceService,
            IMapper mapper,
            ILogger<UpdatePurchaseInvoiceHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _lineItemRepository = lineItemRepository;
            _invoiceService = invoiceService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PurchaseInvoiceDto>> Handle(
            UpdatePurchaseInvoiceCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Starting update of purchase invoice: {InvoiceId}",
                    request.Id);

                var invoice = await _invoiceRepository.GetByIDWithTracking(request.Id);

                if (invoice == null)
                {
                    _logger.LogWarning("Purchase invoice not found: {InvoiceId}", request.Id);
                    throw new NotFoundException(
                        $"Purchase invoice with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                // Validate invoice can be updated (not paid in full)
                if (invoice.PaymentStatus == "PaidInFull")
                {
                    _logger.LogWarning(
                        "Cannot update invoice {InvoiceId} - already paid in full",
                        request.Id);
                    throw new BusinessLogicException(
                        "Cannot update invoice that is already paid in full",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Update invoice header
                invoice.InvoiceDate = request.InvoiceDate;
                invoice.DueDate = request.DueDate;
                invoice.DepositApplied = request.DepositApplied;
                invoice.Notes = request.Notes;
                invoice.UpdatedAt = DateTime.UtcNow;

                // Get existing line items
                var existingLineItems = _lineItemRepository
                    .Get(li => li.InvoiceId == request.Id)
                    .ToList();

                // Remove deleted line items
                var lineItemsToDelete = existingLineItems
                    .Where(existing => !request.LineItems.Any(li => li.Id == existing.Id))
                    .ToList();

                foreach (var lineItem in lineItemsToDelete)
                {
                    await _lineItemRepository.Delete(lineItem.Id);
                }

                // Update existing and add new line items
                foreach (var lineItemDto in request.LineItems)
                {
                    if (lineItemDto.Id != Guid.Empty)
                    {
                        // Update existing
                        var existingLineItem = existingLineItems
                            .FirstOrDefault(li => li.Id == lineItemDto.Id);

                        if (existingLineItem != null)
                        {
                            existingLineItem.Description = lineItemDto.Description;
                            existingLineItem.Quantity = lineItemDto.Quantity;
                            existingLineItem.UnitPrice = lineItemDto.UnitPrice;
                            existingLineItem.TaxAmount = lineItemDto.TaxAmount;
                            existingLineItem.LineTotal = lineItemDto.LineTotal;
                            existingLineItem.UpdatedAt = DateTime.UtcNow;

                            await _lineItemRepository.Update(existingLineItem);
                        }
                    }
                    else
                    {
                        // Add new
                        var newLineItem = new InvoiceLineItem
                        {
                            Id = Guid.NewGuid(),
                            InvoiceId = invoice.Id,
                            POLineItemId = lineItemDto.POLineItemId,
                            Description = lineItemDto.Description,
                            Quantity = lineItemDto.Quantity,
                            UnitPrice = lineItemDto.UnitPrice,
                            TaxAmount = lineItemDto.TaxAmount,
                            LineTotal = lineItemDto.LineTotal,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _lineItemRepository.AddAsync(newLineItem);
                    }
                }

                await _lineItemRepository.SaveChanges();

                // Recalculate totals
                var updatedLineItems = _lineItemRepository
                    .Get(li => li.InvoiceId == request.Id)
                    .ToList();

                invoice.Subtotal = updatedLineItems.Sum(li => li.LineTotal);
                invoice.TaxAmount = updatedLineItems.Sum(li => li.TaxAmount);
                invoice.TotalAmount = invoice.Subtotal + invoice.TaxAmount;

                // Recalculate amount due considering payments
                var totalPaid = invoice.Payments?.Sum(p => p.Amount) ?? 0;
                invoice.AmountDue = invoice.TotalAmount - invoice.DepositApplied - totalPaid;

                // Update payment status
                if (invoice.AmountDue <= 0)
                {
                    invoice.PaymentStatus = "PaidInFull";
                }
                else if (totalPaid > 0 || invoice.DepositApplied > 0)
                {
                    invoice.PaymentStatus = "PartiallyPaid";
                }
                else
                {
                    invoice.PaymentStatus = "Unpaid";
                }

                await _invoiceRepository.Update(invoice);
                await _invoiceRepository.SaveChanges();

                _logger.LogInformation(
                    "Purchase invoice updated successfully: {InvoiceId}",
                    request.Id);

                // Get updated invoice with projection
                var invoiceDto = await _invoiceService.GetInvoiceByIdAsync(
                    invoice.Id,
                    cancellationToken);

                return ResponseViewModel<PurchaseInvoiceDto>.Success(
                    invoiceDto,
                    "Purchase invoice updated successfully");
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
                    "Error updating purchase invoice: {InvoiceId}",
                    request.Id);
                throw new BusinessLogicException(
                    "Failed to update purchase invoice",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}