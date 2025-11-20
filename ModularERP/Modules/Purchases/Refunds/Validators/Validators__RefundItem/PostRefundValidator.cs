using FluentValidation;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundItem;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Validators.Validators__RefundItem
{
    public class PostRefundValidator : AbstractValidator<PostRefundCommand>
    {
        private readonly IGeneralRepository<PurchaseRefund> _refundRepository;
        private readonly IGeneralRepository<RefundLineItem> _refundItemRepository;
        private readonly IGeneralRepository<DebitNote> _debitNoteRepository;

        public PostRefundValidator(
            IGeneralRepository<PurchaseRefund> refundRepository,
            IGeneralRepository<RefundLineItem> refundItemRepository,
            IGeneralRepository<DebitNote> debitNoteRepository)
        {
            _refundRepository = refundRepository;
            _refundItemRepository = refundItemRepository;
            _debitNoteRepository = debitNoteRepository;

            RuleFor(x => x.RefundId)
                .NotEmpty().WithMessage("Refund ID is required")
                .MustAsync(RefundExists).WithMessage("Refund not found")
                .MustAsync(RefundNotAlreadyPosted).WithMessage("Refund has already been posted")
                .MustAsync(RefundHasItems).WithMessage("Refund must have at least one line item");
        }

        private async Task<bool> RefundExists(Guid refundId, CancellationToken cancellationToken)
        {
            return await _refundRepository.AnyAsync(r => r.Id == refundId, cancellationToken);
        }

        private async Task<bool> RefundNotAlreadyPosted(Guid refundId, CancellationToken cancellationToken)
        {
            // Check if DebitNote already exists for this refund
            return !await _debitNoteRepository.AnyAsync(dn => dn.RefundId == refundId, cancellationToken);
        }

        private async Task<bool> RefundHasItems(Guid refundId, CancellationToken cancellationToken)
        {
            return await _refundItemRepository.AnyAsync(ri => ri.RefundId == refundId, cancellationToken);
        }
    }
}
