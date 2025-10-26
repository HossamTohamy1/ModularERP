using FluentValidation;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Validators.Validators_PurchaseOrder
{
    public class ApprovePurchaseOrderValidator : AbstractValidator<ApprovePurchaseOrderCommand>
    {
        public ApprovePurchaseOrderValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Purchase Order ID is required");

            RuleFor(x => x.ApprovedBy)
                .NotEmpty().WithMessage("Approved by user ID is required");

            RuleFor(x => x.ApprovalNotes)
                .MaximumLength(1000).WithMessage("Approval notes cannot exceed 1000 characters");
        }
    }
}