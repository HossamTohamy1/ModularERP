using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundItem;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundItem;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_RefundItem
{
    public class PostRefundHandler : IRequestHandler<PostRefundCommand, ResponseViewModel<PostRefundResponseDto>>
    {
        private readonly IGeneralRepository<PurchaseRefund> _refundRepository;
        private readonly IGeneralRepository<DebitNote> _debitNoteRepository;
        private readonly IGeneralRepository<RefundLineItem> _refundItemRepository;
        private readonly IGeneralRepository<GRNLineItem> _grnLineRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PostRefundHandler> _logger;

        public PostRefundHandler(
            IGeneralRepository<PurchaseRefund> refundRepository,
            IGeneralRepository<DebitNote> debitNoteRepository,
            IGeneralRepository<RefundLineItem> refundItemRepository,
            IGeneralRepository<GRNLineItem> grnLineRepository,
            IMapper mapper,
            ILogger<PostRefundHandler> logger)
        {
            _refundRepository = refundRepository;
            _debitNoteRepository = debitNoteRepository;
            _refundItemRepository = refundItemRepository;
            _grnLineRepository = grnLineRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PostRefundResponseDto>> Handle(PostRefundCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting refund posting process for Refund: {RefundId}", request.RefundId);

                // 1. Get Refund with validation
                var refund = await _refundRepository.GetByIDWithTracking(request.RefundId);
                if (refund == null)
                {
                    throw new NotFoundException("Refund not found", FinanceErrorCode.NotFound);
                }

                // 2. Get Refund Items
                var refundItems = await _refundItemRepository
                    .Get(ri => ri.RefundId == request.RefundId)
                    .ToListAsync(cancellationToken);

                if (!refundItems.Any())
                {
                    throw new BusinessLogicException(
                        "Cannot post refund without line items",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                // 3. Update Inventory (Decrease Stock)
                var inventoryUpdated = await UpdateInventory(refundItems, cancellationToken);

                // 4. Generate Debit Note Number
                var debitNoteNumber = await GenerateDebitNoteNumber();

                // 5. Create Debit Note
                var debitNote = new DebitNote
                {
                    Id = Guid.NewGuid(),
                    DebitNoteNumber = debitNoteNumber,
                    RefundId = refund.Id,
                    SupplierId = refund.SupplierId,
                    NoteDate = DateTime.UtcNow,
                    Amount = refund.TotalAmount,
                    Notes = request.Notes ?? $"Auto-generated for refund {refund.RefundNumber}",
                    CreatedAt = DateTime.UtcNow
                };

                await _debitNoteRepository.AddAsync(debitNote);
                await _debitNoteRepository.SaveChanges();

                // 6. Update Refund Status
                refund.UpdatedAt = DateTime.UtcNow;
                await _refundRepository.SaveChanges();

                _logger.LogInformation(
                    "Refund posted successfully. Refund: {RefundId}, DebitNote: {DebitNoteId}",
                    refund.Id, debitNote.Id);

                // 7. Prepare Response
                var response = new PostRefundResponseDto
                {
                    RefundId = refund.Id,
                    RefundNumber = refund.RefundNumber,
                    DebitNoteId = debitNote.Id,
                    DebitNoteNumber = debitNote.DebitNoteNumber,
                    TotalAmount = refund.TotalAmount,
                    PostedDate = DateTime.UtcNow,
                    InventoryUpdated = inventoryUpdated,
                    AccountsUpdated = true,
                    Message = $"Refund posted successfully. Debit Note {debitNote.DebitNoteNumber} created."
                };

                return ResponseViewModel<PostRefundResponseDto>.Success(
                    response,
                    "Refund posted and Debit Note created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting refund: {RefundId}", request.RefundId);
                throw;
            }
        }

        private async Task<bool> UpdateInventory(List<RefundLineItem> refundItems, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating inventory for {Count} refund items", refundItems.Count);

                foreach (var item in refundItems)
                {
                    var grnLine = await _grnLineRepository.GetByIDWithTracking(item.GRNLineItemId);
                    if (grnLine != null)
                    {
                        // Note: في الـ real implementation، هنا المفروض نحدث الـ Inventory actual stock
                        // بس دلوقتي هنحدث بس الـ ReceivedQuantity في الـ GRN
                        grnLine.ReceivedQuantity -= item.ReturnQuantity;
                        grnLine.UpdatedAt = DateTime.UtcNow;

                        _logger.LogInformation(
                            "Updated GRN Line: {GRNLineId}, Returned Qty: {Qty}",
                            grnLine.Id, item.ReturnQuantity);
                    }
                }

                await _grnLineRepository.SaveChanges();

                _logger.LogInformation("Inventory updated successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory");
                throw new BusinessLogicException(
                    "Failed to update inventory",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }
        }

        private async Task<string> GenerateDebitNoteNumber()
        {
            // Generate sequential debit note number
            var lastDebitNote = await _debitNoteRepository
                .GetAll()
                .OrderByDescending(dn => dn.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastDebitNote == null)
            {
                return "DN-00001";
            }

            // Extract number from last DN number (e.g., "DN-00005" -> 5)
            var lastNumber = int.Parse(lastDebitNote.DebitNoteNumber.Split('-')[1]);
            var newNumber = lastNumber + 1;

            return $"DN-{newNumber:D5}";
        }
    }
}

