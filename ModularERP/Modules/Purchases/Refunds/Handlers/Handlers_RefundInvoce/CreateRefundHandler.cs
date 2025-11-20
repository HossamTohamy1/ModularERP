using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_RefundInvoce
{
    public class CreateRefundHandler : IRequestHandler<CreateRefundCommand, ResponseViewModel<RefundDto>>
    {
        private readonly IGeneralRepository<PurchaseRefund> _refundRepo;
        private readonly IGeneralRepository<DebitNote> _debitNoteRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateRefundHandler> _logger;

        public CreateRefundHandler(
            IGeneralRepository<PurchaseRefund> refundRepo,
            IGeneralRepository<DebitNote> debitNoteRepo,
            IMapper mapper,
            ILogger<CreateRefundHandler> logger)
        {
            _refundRepo = refundRepo;
            _debitNoteRepo = debitNoteRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<RefundDto>> Handle(CreateRefundCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting refund creation for PO: {PurchaseOrderId}", request.PurchaseOrderId);

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

                // Create Refund Entity
                var refund = new PurchaseRefund
                {
                    Id = Guid.NewGuid(),
                    RefundNumber = refundNumber,
                    PurchaseOrderId = request.PurchaseOrderId,
                    SupplierId = request.SupplierId,
                    RefundDate = request.RefundDate,
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
                        SupplierId = request.SupplierId,
                        NoteDate = DateTime.UtcNow,
                        Amount = totalAmount,
                        Notes = $"Auto-generated for Refund: {refundNumber}",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _debitNoteRepo.AddAsync(debitNote);
                    _logger.LogInformation("Created Debit Note: {DebitNoteNumber} for Refund: {RefundNumber}", debitNoteNumber, refundNumber);
                }

                await _refundRepo.SaveChanges();

                _logger.LogInformation("Successfully created refund: {RefundNumber}", refundNumber);

                // Project to DTO using AutoMapper
                var refundDto = await _refundRepo.GetAll()
                    .Where(r => r.Id == refund.Id)
                    .Select(r => _mapper.Map<RefundDto>(r))
                    .FirstOrDefaultAsync(cancellationToken);

                return ResponseViewModel<RefundDto>.Success(refundDto!, "Refund created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund for PO: {PurchaseOrderId}", request.PurchaseOrderId);
                throw;
            }
        }
    }
}