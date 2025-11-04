using FluentValidation;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Validators.Validators_GRNItem
{
    public class UpdateGRNLineItemValidator : AbstractValidator<UpdateGRNLineItemDto>
    {
        public UpdateGRNLineItemValidator()
        {
            RuleFor(x => x.POLineItemId)
                .NotEmpty().WithMessage("PO Line Item ID is required");

            RuleFor(x => x.ReceivedQuantity)
                .GreaterThan(0).WithMessage("Received Quantity must be greater than zero");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Line item notes cannot exceed 500 characters");
        }
    }
}