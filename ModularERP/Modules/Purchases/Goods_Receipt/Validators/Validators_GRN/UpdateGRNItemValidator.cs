using FluentValidation;
using ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRNItem;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Validators.Validators_GRN
{
    public class UpdateGRNItemValidator : AbstractValidator<UpdateGRNItemCommand>
    {
        public UpdateGRNItemValidator()
        {
            RuleFor(x => x.GRNId)
                .NotEmpty()
                .WithMessage("GRN ID is required");

            RuleFor(x => x.ItemId)
                .NotEmpty()
                .WithMessage("Item ID is required");

            RuleFor(x => x.POLineItemId)
                .NotEmpty()
                .WithMessage("PO Line Item ID is required");

            RuleFor(x => x.ReceivedQuantity)
                .GreaterThan(0)
                .WithMessage("Received quantity must be greater than zero");
        }
    }
}