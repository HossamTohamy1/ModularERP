using FluentValidation;
using ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceRule;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceRule
{
    public class GetPriceListRulesQueryValidator : AbstractValidator<GetPriceListRulesQuery>
    {
        public GetPriceListRulesQueryValidator()
        {
            RuleFor(x => x.PriceListId)
                .NotEmpty()
                .WithMessage("Price list ID is required");
        }
    }

}
