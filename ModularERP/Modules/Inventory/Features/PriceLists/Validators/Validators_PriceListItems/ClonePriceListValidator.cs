using FluentValidation;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceListItems
{
    public class ClonePriceListValidator : AbstractValidator<ClonePriceListDto>
    {
        public ClonePriceListValidator()
        {
            RuleFor(x => x.SourcePriceListId)
                .NotEmpty().WithMessage("Source price list is required");

            RuleFor(x => x.NewName)
                .NotEmpty().WithMessage("New name is required")
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");
        }
    }
}
