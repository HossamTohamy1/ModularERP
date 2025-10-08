using FluentValidation;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceRule;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceRule
{
    public class DeletePriceListRuleCommandValidator : AbstractValidator<DeletePriceListRuleCommand>
    {
        public DeletePriceListRuleCommandValidator()
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
