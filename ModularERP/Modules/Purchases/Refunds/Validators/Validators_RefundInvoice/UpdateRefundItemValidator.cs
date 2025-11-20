using FluentValidation;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundItem;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Validators.Validators_RefundInvoice
{
    public class UpdateRefundItemValidator : AbstractValidator<UpdateRefundItemCommand>
    {
        private readonly IGeneralRepository<RefundLineItem> _refundItemRepository;
        private readonly IGeneralRepository<GRNLineItem> _grnLineRepository;

        public UpdateRefundItemValidator(
            IGeneralRepository<RefundLineItem> refundItemRepository,
            IGeneralRepository<GRNLineItem> grnLineRepository)
        {
            _refundItemRepository = refundItemRepository;
            _grnLineRepository = grnLineRepository;

            RuleFor(x => x.ItemId)
                .NotEmpty().WithMessage("Item ID is required")
                .MustAsync(ItemExists).WithMessage("Refund item not found");

            RuleFor(x => x.ReturnQuantity)
                .GreaterThan(0).WithMessage("Return quantity must be greater than zero")
                .MustAsync(ValidateReturnQuantity).WithMessage("Return quantity exceeds available quantity");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to zero");
        }

        private async Task<bool> ItemExists(Guid itemId, CancellationToken cancellationToken)
        {
            return await _refundItemRepository.AnyAsync(i => i.Id == itemId, cancellationToken);
        }

        private async Task<bool> ValidateReturnQuantity(UpdateRefundItemCommand command, decimal quantity, CancellationToken cancellationToken)
        {
            var refundItem = await _refundItemRepository.GetByID(command.ItemId);
            if (refundItem == null) return false;

            var grnLine = await _grnLineRepository.GetByID(refundItem.GRNLineItemId);
            if (grnLine == null) return false;

            var existingReturns = _refundItemRepository
                .Get(r => r.GRNLineItemId == refundItem.GRNLineItemId && r.Id != command.ItemId)
                .Sum(r => r.ReturnQuantity);

            var availableQty = grnLine.ReceivedQuantity - existingReturns;
            return quantity <= availableQty;
        }
    }
}
