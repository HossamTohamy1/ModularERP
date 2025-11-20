using FluentValidation;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundItem;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Validators.Validators_RefundInvoice
{
    public class DeleteRefundItemValidator : AbstractValidator<DeleteRefundItemCommand>
    {
        private readonly IGeneralRepository<RefundLineItem> _refundItemRepository;

        public DeleteRefundItemValidator(IGeneralRepository<RefundLineItem> refundItemRepository)
        {
            _refundItemRepository = refundItemRepository;

            RuleFor(x => x.ItemId)
                .NotEmpty().WithMessage("Item ID is required")
                .MustAsync(ItemExists).WithMessage("Refund item not found");
        }

        private async Task<bool> ItemExists(Guid itemId, CancellationToken cancellationToken)
        {
            return await _refundItemRepository.AnyAsync(i => i.Id == itemId, cancellationToken);
        }
    }
}