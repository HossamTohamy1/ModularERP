using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_Refund
{
    public class DeleteRefundHandler : IRequestHandler<DeleteRefundCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<PurchaseRefund> _refundRepo;
        private readonly IGeneralRepository<DebitNote> _debitNoteRepo;
        private readonly IGeneralRepository<RefundLineItem> _lineItemRepo;
        private readonly ILogger<DeleteRefundHandler> _logger;

        public DeleteRefundHandler(
            IGeneralRepository<PurchaseRefund> refundRepo,
            IGeneralRepository<DebitNote> debitNoteRepo,
            IGeneralRepository<RefundLineItem> lineItemRepo,
            ILogger<DeleteRefundHandler> logger)
        {
            _refundRepo = refundRepo;
            _debitNoteRepo = debitNoteRepo;
            _lineItemRepo = lineItemRepo;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteRefundCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting refund: {RefundId}", request.Id);

                var refund = await _refundRepo.GetByID(request.Id);
                if (refund == null)
                {
                    throw new NotFoundException(
                        $"Refund with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                // Delete associated debit note
                var debitNote = _debitNoteRepo.GetAll()
                    .FirstOrDefault(dn => dn.RefundId == request.Id);
                if (debitNote != null)
                {
                    await _debitNoteRepo.Delete(debitNote.Id);
                    _logger.LogInformation("Deleted associated Debit Note: {DebitNoteId}", debitNote.Id);
                }

                // Delete line items
                var lineItems = _lineItemRepo.GetAll()
                    .Where(li => li.RefundId == request.Id);
                foreach (var lineItem in lineItems)
                {
                    await _lineItemRepo.Delete(lineItem.Id);
                }

                // Delete refund (soft delete)
                await _refundRepo.Delete(request.Id);
                await _refundRepo.SaveChanges();

                _logger.LogInformation("Successfully deleted refund: {RefundId}", request.Id);

                return ResponseViewModel<bool>.Success(true, "Refund deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting refund: {RefundId}", request.Id);
                throw;
            }
        }
    }
}
