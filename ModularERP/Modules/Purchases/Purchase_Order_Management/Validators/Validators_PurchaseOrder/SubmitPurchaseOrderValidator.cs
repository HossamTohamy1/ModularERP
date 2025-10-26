using FluentValidation;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Validators.Validators_PurchaseOrder
{
    public class SubmitPurchaseOrderValidator : AbstractValidator<SubmitPurchaseOrderCommand>
    {
        public SubmitPurchaseOrderValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Purchase Order ID is required");

            RuleFor(x => x.SubmittedBy)
                .NotEmpty().WithMessage("Submitted by user ID is required");
        }
    }
}
