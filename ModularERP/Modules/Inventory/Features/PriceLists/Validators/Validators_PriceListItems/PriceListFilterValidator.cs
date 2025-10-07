using FluentValidation;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceListItems
{
    public class PriceListFilterValidator : AbstractValidator<PriceListFilterDto>
    {
        public PriceListFilterValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");

            RuleFor(x => x.Type)
                .IsInEnum().When(x => x.Type.HasValue)
                .WithMessage("Invalid price list type");

            RuleFor(x => x.Status)
                .IsInEnum().When(x => x.Status.HasValue)
                .WithMessage("Invalid status");

            RuleFor(x => x.CurrencyCode)
                .Length(3).When(x => !string.IsNullOrEmpty(x.CurrencyCode))
                .WithMessage("Currency code must be exactly 3 characters");
        }
    }
}
