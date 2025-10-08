using FluentValidation;
using ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceRule;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceRule
{
    public class GetPriceListRuleByIdQueryValidator : AbstractValidator<GetPriceListRuleByIdQuery>
    {
        public GetPriceListRuleByIdQueryValidator()
        {
            RuleFor(x => x.PriceListId)
                .NotEmpty()
                .WithMessage("Price list ID is required");

            RuleFor(x => x.RuleId)
                .NotEmpty()
                .WithMessage("Rule ID is required");
        }
    }
}
