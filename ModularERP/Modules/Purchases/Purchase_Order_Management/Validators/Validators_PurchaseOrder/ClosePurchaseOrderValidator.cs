using FluentValidation;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Validators.Validators_PurchaseOrder
{
    public class ClosePurchaseOrderValidator : AbstractValidator<ClosePurchaseOrderCommand>
    {
        public ClosePurchaseOrderValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Purchase Order ID is required");

            RuleFor(x => x.ClosedBy)
                .NotEmpty().WithMessage("Closed by user ID is required");
        }
    }

}
