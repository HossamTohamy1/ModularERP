using FluentValidation;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceRule;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceRule
{
    public class CreatePriceListRuleCommandValidator : AbstractValidator<CreatePriceListRuleCommand>
    {
        public CreatePriceListRuleCommandValidator()
        {
            RuleFor(x => x.PriceListId)
                .NotEmpty()
                .WithMessage("Price list ID is required");

            RuleFor(x => x.Data)
                .NotNull()
                .WithMessage("Rule data is required")
                .SetValidator(new CreatePriceListRuleDTOValidator());
        }
    }
}
