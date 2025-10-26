using FluentValidation;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Validators.Validators_PurchaseOrder
{
    public class CancelPurchaseOrderValidator : AbstractValidator<CancelPurchaseOrderCommand>
    {
        public CancelPurchaseOrderValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Purchase Order ID is required");

            RuleFor(x => x.CancellationReason)
                .NotEmpty().WithMessage("Cancellation reason is required")
                .MinimumLength(10).WithMessage("Cancellation reason must be at least 10 characters")
                .MaximumLength(500).WithMessage("Cancellation reason cannot exceed 500 characters");
        }
    }
}
