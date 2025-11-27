using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Purchases_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commend_InvocieItem;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_InvocieItem
{
    public class AddInvoiceItemHandler : IRequestHandler<AddInvoiceItemCommand, ResponseViewModel<InvoiceLineItemResponse>>
    {
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepository;
        private readonly IGeneralRepository<InvoiceLineItem> _lineItemRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AddInvoiceItemHandler> _logger;

        public AddInvoiceItemHandler(
            IGeneralRepository<PurchaseInvoice> invoiceRepository,
            IGeneralRepository<InvoiceLineItem> lineItemRepository,
            IMapper mapper,
            ILogger<AddInvoiceItemHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _lineItemRepository = lineItemRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<InvoiceLineItemResponse>> Handle(AddInvoiceItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Adding item to invoice {InvoiceId}", request.InvoiceId);

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

                if (invoice.PaymentStatus == PaymentStatus.PaidInFull)
                {
                    throw new BusinessLogicException(
                        "Cannot add items to fully paid invoices",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError
                    );
                }

                // Check if item already exists
                var existingItem = await _lineItemRepository
                    .GetAll()
                    .AnyAsync(li => li.InvoiceId == request.InvoiceId && li.POLineItemId == request.Request.POLineItemId, cancellationToken);

                if (existingItem)
                {
                    throw new BusinessLogicException(
                        "This PO line item already exists in the invoice",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError
                    );
                }

                // Create line item
                var lineItem = _mapper.Map<InvoiceLineItem>(request.Request);
                lineItem.Id = Guid.NewGuid();
                lineItem.InvoiceId = request.InvoiceId;
                lineItem.CreatedAt = DateTime.UtcNow;

                await _lineItemRepository.AddAsync(lineItem);

                // Update invoice totals
                invoice.Subtotal += lineItem.Quantity * lineItem.UnitPrice;
                invoice.TaxAmount += lineItem.TaxAmount;
                invoice.TotalAmount = invoice.Subtotal + invoice.TaxAmount;
                invoice.AmountDue = invoice.TotalAmount - invoice.DepositApplied;
                invoice.UpdatedAt = DateTime.UtcNow;

                await _invoiceRepository.Update(invoice);
                await _lineItemRepository.SaveChanges();

                _logger.LogInformation("Successfully added item to invoice {InvoiceId}", request.InvoiceId);

                var response = _mapper.Map<InvoiceLineItemResponse>(lineItem);

                return ResponseViewModel<InvoiceLineItemResponse>.Success(
                    response,
                    "Invoice item added successfully"
                );
            }
            catch (BaseApplicationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding item to invoice {InvoiceId}", request.InvoiceId);

                return ResponseViewModel<InvoiceLineItemResponse>.Error(
                    "An error occurred while adding the invoice item",
                    FinanceErrorCode.InternalServerError
                );
            }
        }
    }
}