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
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_InvocieItem
{
    public class CreateInvoiceFromPOHandler : IRequestHandler<CreateInvoiceFromPOCommand, ResponseViewModel<InvoiceResponse>>
    {
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepository;
        private readonly IGeneralRepository<POLineItem> _poLineItemRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateInvoiceFromPOHandler> _logger;

        public CreateInvoiceFromPOHandler(
            IGeneralRepository<PurchaseOrder> poRepository,
            IGeneralRepository<PurchaseInvoice> invoiceRepository,
            IGeneralRepository<POLineItem> poLineItemRepository,
            IMapper mapper,
            ILogger<CreateInvoiceFromPOHandler> logger)
        {
            _poRepository = poRepository;
            _invoiceRepository = invoiceRepository;
            _poLineItemRepository = poLineItemRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<InvoiceResponse>> Handle(CreateInvoiceFromPOCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating invoice from Purchase Order ID: {PurchaseOrderId}", request.PurchaseOrderId);

                // Validate Purchase Order exists
                var purchaseOrder = await _poRepository
                    .GetAll()
                    .Where(po => po.Id == request.PurchaseOrderId)
                    .Select(po => new
                    {
                        po.Id,
                        po.PONumber,
                        po.CompanyId,
                        po.SupplierId,
                        po.DocumentStatus
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (purchaseOrder == null)
                {
                    _logger.LogWarning("Purchase Order not found with ID: {PurchaseOrderId}", request.PurchaseOrderId);
                    throw new NotFoundException(
                        $"Purchase Order with ID {request.PurchaseOrderId} not found",
                        FinanceErrorCode.NotFound
                    );
                }

                if (purchaseOrder.DocumentStatus != DocumentStatus.Approved)
                {
                    throw new BusinessLogicException(
                        "Can only create invoice from approved purchase orders",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError
                    );
                }

                // Get PO Line Items
                var poLineItems = await _poLineItemRepository
                    .GetAll()
                    .Where(li => li.PurchaseOrderId == request.PurchaseOrderId)
                    .Select(li => new
                    {
                        li.Id,
                        li.Description,
                        li.Quantity,
                        li.UnitPrice,
                        li.TaxAmount
                    })
                    .ToListAsync(cancellationToken);

                if (!poLineItems.Any())
                {
                    throw new BusinessLogicException(
                        "Cannot create invoice from purchase order with no line items",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError
                    );
                }

                // Calculate totals
                decimal subtotal = poLineItems.Sum(li => li.Quantity * li.UnitPrice);
                decimal taxAmount = poLineItems.Sum(li => li.TaxAmount);
                decimal totalAmount = subtotal + taxAmount;
                decimal amountDue = totalAmount - request.Request.DepositApplied;

                // Generate Invoice Number
                var lastInvoice = await _invoiceRepository
                    .GetAll()
                    .OrderByDescending(i => i.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                string invoiceNumber = GenerateInvoiceNumber(lastInvoice?.InvoiceNumber);

                // Create Invoice
                var invoice = new PurchaseInvoice
                {
                    Id = Guid.NewGuid(),
                    InvoiceNumber = invoiceNumber,
                    PurchaseOrderId = request.PurchaseOrderId,
                    CompanyId = purchaseOrder.CompanyId,
                    SupplierId = purchaseOrder.SupplierId,
                    InvoiceDate = request.Request.InvoiceDate,
                    DueDate = request.Request.DueDate,
                    Subtotal = subtotal,
                    TaxAmount = taxAmount,
                    TotalAmount = totalAmount,
                    DepositApplied = request.Request.DepositApplied,
                    AmountDue = amountDue,
                    PaymentStatus = amountDue == 0
                    ? PaymentStatus.PaidInFull
                    : (request.Request.DepositApplied > 0
                        ? PaymentStatus.PartiallyPaid
                        : PaymentStatus.Unpaid),
                    Notes = request.Request.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                await _invoiceRepository.AddAsync(invoice);

                // Create Invoice Line Items
                var invoiceLineItems = poLineItems.Select(li => new InvoiceLineItem
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    POLineItemId = li.Id,
                    Description = li.Description,
                    Quantity = li.Quantity,
                    UnitPrice = li.UnitPrice,
                    TaxAmount = li.TaxAmount,
                    LineTotal = (li.Quantity * li.UnitPrice) + li.TaxAmount,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                var lineItemRepository = _poLineItemRepository as IGeneralRepository<InvoiceLineItem>;
                if (lineItemRepository != null)
                {
                    await lineItemRepository.AddRangeAsync(invoiceLineItems);
                }

                await _invoiceRepository.SaveChanges();

                _logger.LogInformation("Successfully created invoice {InvoiceNumber} from Purchase Order {OrderNumber}",
                    invoiceNumber, purchaseOrder.PONumber);

                // Return response using projection
                var createdInvoice = await _invoiceRepository
                    .GetAll()
                    .Where(i => i.Id == invoice.Id)
                    .Select(i => new InvoiceResponse
                    {
                        Id = i.Id,
                        InvoiceNumber = i.InvoiceNumber,
                        PurchaseOrderId = i.PurchaseOrderId,
                        PurchaseOrderNumber = purchaseOrder.PONumber,
                        CompanyId = i.CompanyId,
                        CompanyName = i.Company.Name??"",
                        SupplierId = i.SupplierId,
                        SupplierName = i.Supplier.Name??"",
                        InvoiceDate = i.InvoiceDate,
                        DueDate = i.DueDate,
                        Subtotal = i.Subtotal,
                        TaxAmount = i.TaxAmount,
                        TotalAmount = i.TotalAmount,
                        DepositApplied = i.DepositApplied,
                        AmountDue = i.AmountDue,
                        PaymentStatus = i.PaymentStatus.ToString(),
                        Notes = i.Notes,
                        CreatedAt = i.CreatedAt,
                        LineItems = invoiceLineItems.Select(li => new InvoiceLineItemResponse
                        {
                            Id = li.Id,
                            InvoiceId = li.InvoiceId,
                            POLineItemId = li.POLineItemId,
                            Description = li.Description,
                            Quantity = li.Quantity,
                            UnitPrice = li.UnitPrice,
                            TaxAmount = li.TaxAmount,
                            LineTotal = li.LineTotal
                        }).ToList()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                return ResponseViewModel<InvoiceResponse>.Success(
                    createdInvoice!,
                    "Invoice created successfully"
                );
            }
            catch (BaseApplicationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating invoice from Purchase Order ID: {PurchaseOrderId}",
                    request.PurchaseOrderId);

                return ResponseViewModel<InvoiceResponse>.Error(
                    "An error occurred while creating the invoice",
                    FinanceErrorCode.InternalServerError
                );
            }
        }

        private string GenerateInvoiceNumber(string? lastInvoiceNumber)
        {
            if (string.IsNullOrEmpty(lastInvoiceNumber))
            {
                return $"INV-{DateTime.UtcNow:yyyyMM}-0001";
            }

            var parts = lastInvoiceNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
            {
                var currentMonth = DateTime.UtcNow.ToString("yyyyMM");
                var lastMonth = parts[1];

                if (currentMonth == lastMonth)
                {
                    return $"INV-{currentMonth}-{(lastNumber + 1):D4}";
                }
            }

            return $"INV-{DateTime.UtcNow:yyyyMM}-0001";
        }
    }
}
