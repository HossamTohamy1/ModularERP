using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_RefundInvoce
{
    public class CreateRefundFromInvoiceHandler : IRequestHandler<CreateRefundFromInvoiceCommand, ResponseViewModel<RefundDto>>
    {
        private readonly IGeneralRepository<PurchaseRefund> _refundRepo;
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepo;
        private readonly IGeneralRepository<DebitNote> _debitNoteRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateRefundFromInvoiceHandler> _logger;

        public CreateRefundFromInvoiceHandler(
            IGeneralRepository<PurchaseRefund> refundRepo,
            IGeneralRepository<PurchaseInvoice> invoiceRepo,
            IGeneralRepository<DebitNote> debitNoteRepo,
            IMapper mapper,
            ILogger<CreateRefundFromInvoiceHandler> logger)
        {
            _refundRepo = refundRepo;
            _invoiceRepo = invoiceRepo;
            _debitNoteRepo = debitNoteRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<RefundDto>> Handle(CreateRefundFromInvoiceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating refund from Purchase Invoice: {InvoiceId}", request.InvoiceId);

                // Validate Invoice exists
                var invoice = await _invoiceRepo.GetByID(request.InvoiceId);
                if (invoice == null)
                {
                    throw new NotFoundException(
                        $"Purchase Invoice with ID {request.InvoiceId} not found",
                        FinanceErrorCode.NotFound);
                }

                // Validation
                if (!request.LineItems.Any())
                {
                    throw new ValidationException(
                        "Refund must contain at least one line item",
                        new Dictionary<string, string[]> { { "LineItems", new[] { "At least one line item is required" } } },
                        "Purchases");
                }

                // Generate Refund Number
                var refundCount = await _refundRepo.GetAll().CountAsync(cancellationToken);
                var refundNumber = $"REF-{DateTime.UtcNow:yyyyMMdd}-{refundCount + 1:D5}";

                // Calculate Total Amount
                var totalAmount = request.LineItems.Sum(item => item.ReturnQuantity * item.UnitPrice);

                // Create Refund Entity (linked to PO from Invoice)
                var refund = new PurchaseRefund
                {
                    Id = Guid.NewGuid(),
                    RefundNumber = refundNumber,
                    PurchaseOrderId = invoice.PurchaseOrderId, // Link to PO
                    SupplierId = invoice.SupplierId,
                    RefundDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    Reason = request.Reason,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                // Create Line Items
                foreach (var lineItem in request.LineItems)
                {
                    var refundLine = new RefundLineItem
                    {
                        Id = Guid.NewGuid(),
                        RefundId = refund.Id,
                        GRNLineItemId = lineItem.GRNLineItemId,
                        ReturnQuantity = lineItem.ReturnQuantity,
                        UnitPrice = lineItem.UnitPrice,
                        LineTotal = lineItem.ReturnQuantity * lineItem.UnitPrice,
                        CreatedAt = DateTime.UtcNow
                    };
                    refund.LineItems.Add(refundLine);
                }

                await _refundRepo.AddAsync(refund);

                // Create Debit Note if requested
                if (request.CreateDebitNote)
                {
                    var debitNoteCount = await _debitNoteRepo.GetAll().CountAsync(cancellationToken);
                    var debitNoteNumber = $"DN-{DateTime.UtcNow:yyyyMMdd}-{debitNoteCount + 1:D5}";

                    var debitNote = new DebitNote
                    {
                        Id = Guid.NewGuid(),
                        DebitNoteNumber = debitNoteNumber,
                        RefundId = refund.Id,
                        SupplierId = invoice.SupplierId,
                        NoteDate = DateTime.UtcNow,
                        Amount = totalAmount,
                        Notes = $"Auto-generated for Refund: {refundNumber} (Invoice: {invoice.InvoiceNumber})",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _debitNoteRepo.AddAsync(debitNote);
                    _logger.LogInformation("Created Debit Note: {DebitNoteNumber} for Refund: {RefundNumber}", debitNoteNumber, refundNumber);
                }

                await _refundRepo.SaveChanges();

                _logger.LogInformation("Successfully created refund: {RefundNumber} from Invoice: {InvoiceId}",
                    refundNumber, request.InvoiceId);

                // Project to DTO using AutoMapper
                var refundDto = await _refundRepo.GetAll()
                    .Where(r => r.Id == refund.Id)
                    .Select(r => _mapper.Map<RefundDto>(r))
                    .FirstOrDefaultAsync(cancellationToken);

                return ResponseViewModel<RefundDto>.Success(refundDto!, "Refund created successfully from Purchase Invoice");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund from Invoice: {InvoiceId}", request.InvoiceId);
                throw;
            }
        }
    }
}

