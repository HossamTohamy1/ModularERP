using FluentValidation;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceList;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceList
{
    public class UpdatePriceListItemValidator : AbstractValidator<UpdatePriceListItemDto>
    {
        public UpdatePriceListItemValidator()
        {
            RuleFor(x => x.BasePrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.BasePrice.HasValue)
                .WithMessage("Base price must be greater than or equal to 0");

            RuleFor(x => x.ListPrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.ListPrice.HasValue)
                .WithMessage("List price must be greater than or equal to 0");

            RuleFor(x => x.DiscountValue)
                .GreaterThanOrEqualTo(0)
                .When(x => x.DiscountValue.HasValue)
                .WithMessage("Discount value must be greater than or equal to 0");

            RuleFor(x => x.DiscountType)
                .Must(x => x == "%" || x == "Fixed" || string.IsNullOrEmpty(x))
                .When(x => !string.IsNullOrEmpty(x.DiscountType))
                .WithMessage("Discount type must be '%' or 'Fixed'");

            RuleFor(x => x.ValidTo)
                .GreaterThan(x => x.ValidFrom)
                .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue)
                .WithMessage("ValidTo must be greater than ValidFrom");
        }
    }
}
