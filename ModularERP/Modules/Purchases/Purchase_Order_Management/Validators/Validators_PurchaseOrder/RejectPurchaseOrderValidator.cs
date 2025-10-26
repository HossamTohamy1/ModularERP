using FluentValidation;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Validators.Validators_PurchaseOrder
{
    public class RejectPurchaseOrderValidator : AbstractValidator<RejectPurchaseOrderCommand>
    {
        public RejectPurchaseOrderValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Purchase Order ID is required");

            RuleFor(x => x.RejectedBy)
                .NotEmpty().WithMessage("Rejected by user ID is required");

            RuleFor(x => x.RejectionReason)
                .NotEmpty().WithMessage("Rejection reason is required")
                .MinimumLength(10).WithMessage("Rejection reason must be at least 10 characters")
                .MaximumLength(500).WithMessage("Rejection reason cannot exceed 500 characters");
        }
    }
}       