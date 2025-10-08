using FluentValidation;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceList;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceList
{
    public class BulkCreatePriceListItemValidator : AbstractValidator<BulkCreatePriceListItemDto>
    {
        public BulkCreatePriceListItemValidator()
        {
            RuleFor(x => x.Items)
                .NotNull()
                .NotEmpty()
                .WithMessage("Items list cannot be empty");

            RuleForEach(x => x.Items)
                .SetValidator(new CreatePriceListItemValidator());
        }
    }
}
