using FluentValidation;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceListItems
{
    public class CreatePriceListValidator : AbstractValidator<CreatePriceListDto>
    {
        public CreatePriceListValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("Company is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Price list name is required")
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid price list type");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().WithMessage("Currency code is required")
                .Length(3).WithMessage("Currency code must be exactly 3 characters");

            RuleFor(x => x.ValidTo)
                .GreaterThan(x => x.ValidFrom)
                .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue)
                .WithMessage("Valid To date must be after Valid From date");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid status");
        }
    }

}
