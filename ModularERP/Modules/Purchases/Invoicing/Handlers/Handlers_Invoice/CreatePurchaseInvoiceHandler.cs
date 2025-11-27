using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Purchases_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commands_Invoice;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_Invoice;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Modules.Purchases.Invoicing.Services;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_Invoice
{
    public class CreatePurchaseInvoiceHandler : IRequestHandler<CreatePurchaseInvoiceCommand, ResponseViewModel<PurchaseInvoiceDto>>
    {
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepository;
        private readonly IGeneralRepository<InvoiceLineItem> _lineItemRepository;
        private readonly IPurchaseInvoiceService _invoiceService;
        private readonly IMapper _mapper;
        private readonly ILogger<CreatePurchaseInvoiceHandler> _logger;

        public CreatePurchaseInvoiceHandler(
            IGeneralRepository<PurchaseInvoice> invoiceRepository,
            IGeneralRepository<InvoiceLineItem> lineItemRepository,
            IPurchaseInvoiceService invoiceService,
            IMapper mapper,
            ILogger<CreatePurchaseInvoiceHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _lineItemRepository = lineItemRepository;
            _invoiceService = invoiceService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PurchaseInvoiceDto>> Handle(
            CreatePurchaseInvoiceCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Starting creation of purchase invoice for PO: {POId}",
                    request.PurchaseOrderId);

                // Validate Purchase Order exists and is approved
                await _invoiceService.ValidatePurchaseOrderAsync(
                    request.PurchaseOrderId,
                    cancellationToken);

                // Generate Invoice Number
                var invoiceNumber = await _invoiceService.GenerateInvoiceNumberAsync(
                    request.CompanyId,
                    cancellationToken);

                // Calculate totals
                var subtotal = request.LineItems.Sum(li => li.LineTotal);
                var taxAmount = request.LineItems.Sum(li => li.TaxAmount);
                var totalAmount = subtotal + taxAmount;
                var amountDue = totalAmount - request.DepositApplied;

                // Create Invoice entity
                var invoice = new PurchaseInvoice
                {
                    Id = Guid.NewGuid(),
                    InvoiceNumber = invoiceNumber,
                    PurchaseOrderId = request.PurchaseOrderId,
                    CompanyId = request.CompanyId,
                    SupplierId = request.SupplierId,
                    InvoiceDate = request.InvoiceDate,
                    DueDate = request.DueDate,
                    Subtotal = subtotal,
                    TaxAmount = taxAmount,
                    TotalAmount = totalAmount,
                    DepositApplied = request.DepositApplied,
                    AmountDue = amountDue,
                    PaymentStatus = amountDue == 0 ? PaymentStatus.PaidInFull :
                                   request.DepositApplied > 0 ? PaymentStatus.PartiallyPaid : PaymentStatus.Unpaid,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                await _invoiceRepository.AddAsync(invoice);
                await _invoiceRepository.SaveChanges();

                _logger.LogInformation(
                    "Purchase invoice created: {InvoiceId}, Number: {InvoiceNumber}",
                    invoice.Id, invoiceNumber);

                // Create Line Items
                var lineItems = request.LineItems.Select(li => new InvoiceLineItem
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    POLineItemId = li.POLineItemId,
                    Description = li.Description,
                    Quantity = li.Quantity,
                    UnitPrice = li.UnitPrice,
                    TaxAmount = li.TaxAmount,
                    LineTotal = li.LineTotal,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _lineItemRepository.AddRangeAsync(lineItems);
                await _lineItemRepository.SaveChanges();

                _logger.LogInformation(
                    "Created {Count} line items for invoice: {InvoiceId}",
                    lineItems.Count, invoice.Id);

                // Update PO Payment Status
                await _invoiceService.UpdatePurchaseOrderPaymentStatusAsync(
                    request.PurchaseOrderId,
                    cancellationToken);

                // Map to DTO using projection
                var invoiceDto = await _invoiceService.GetInvoiceByIdAsync(
                    invoice.Id,
                    cancellationToken);

                _logger.LogInformation(
                    "Successfully created purchase invoice: {InvoiceId}",
                    invoice.Id);

                return ResponseViewModel<PurchaseInvoiceDto>.Success(
                    invoiceDto,
                    "Purchase invoice created successfully");
            }
            catch (BusinessLogicException ex)
            {
                _logger.LogWarning(ex,
                    "Business logic error creating invoice for PO: {POId}",
                    request.PurchaseOrderId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error creating purchase invoice for PO: {POId}",
                    request.PurchaseOrderId);
                throw new BusinessLogicException(
                    "Failed to create purchase invoice",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}