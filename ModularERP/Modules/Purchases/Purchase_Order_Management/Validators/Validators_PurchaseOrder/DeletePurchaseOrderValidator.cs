using FluentValidation;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Validators.Validators_PurchaseOrder
{
    public class DeletePurchaseOrderValidator : AbstractValidator<DeletePurchaseOrderCommand>
    {
        public DeletePurchaseOrderValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Purchase Order ID is required");
        }
    }
}
