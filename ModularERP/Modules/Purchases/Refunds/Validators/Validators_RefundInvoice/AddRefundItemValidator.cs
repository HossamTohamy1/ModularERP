using FluentValidation;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundItem;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Validators.Validators_RefundInvoice
{
    public class AddRefundItemValidator : AbstractValidator<AddRefundItemCommand>
    {
        private readonly IGeneralRepository<PurchaseRefund> _refundRepository;
        private readonly IGeneralRepository<GRNLineItem> _grnLineRepository;
        private readonly IGeneralRepository<RefundLineItem> _refundItemRepository;

        public AddRefundItemValidator(
            IGeneralRepository<PurchaseRefund> refundRepository,
            IGeneralRepository<GRNLineItem> grnLineRepository,
            IGeneralRepository<RefundLineItem> refundItemRepository)
        {
            _refundRepository = refundRepository;
            _grnLineRepository = grnLineRepository;
            _refundItemRepository = refundItemRepository;

            RuleFor(x => x.RefundId)
                .NotEmpty().WithMessage("Refund ID is required")
                .MustAsync(RefundExists).WithMessage("Refund not found");

            RuleFor(x => x.GRNLineItemId)
                .NotEmpty().WithMessage("GRN Line Item ID is required")
                .MustAsync(GRNLineExists).WithMessage("GRN Line Item not found");

            RuleFor(x => x.ReturnQuantity)
                .GreaterThan(0).WithMessage("Return quantity must be greater than zero")
                .MustAsync(ValidateReturnQuantity).WithMessage("Return quantity exceeds available quantity");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to zero");
        }

        private async Task<bool> RefundExists(Guid refundId, CancellationToken cancellationToken)
        {
            return await _refundRepository.AnyAsync(r => r.Id == refundId, cancellationToken);
        }

        private async Task<bool> GRNLineExists(Guid grnLineId, CancellationToken cancellationToken)
        {
            return await _grnLineRepository.AnyAsync(g => g.Id == grnLineId, cancellationToken);
        }

        private async Task<bool> ValidateReturnQuantity(AddRefundItemCommand command, decimal quantity, CancellationToken cancellationToken)
        {
            var grnLine = await _grnLineRepository.GetByID(command.GRNLineItemId);
            if (grnLine == null) return false;

            var existingReturns = _refundItemRepository
                .Get(r => r.GRNLineItemId == command.GRNLineItemId && r.RefundId != command.RefundId)
                .Sum(r => r.ReturnQuantity);

            var availableQty = grnLine.ReceivedQuantity - existingReturns;
            return quantity <= availableQty;
        }
    }

}
