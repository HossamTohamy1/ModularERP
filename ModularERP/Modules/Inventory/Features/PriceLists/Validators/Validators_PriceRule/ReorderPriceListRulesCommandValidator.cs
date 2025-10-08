using FluentValidation;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceRule;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceRule
{
    public class ReorderPriceListRulesCommandValidator : AbstractValidator<ReorderPriceListRulesCommand>
    {
        public ReorderPriceListRulesCommandValidator()
        {
            RuleFor(x => x.PriceListId)
                .NotEmpty()
                .WithMessage("Price list ID is required");

            RuleFor(x => x.Data)
                .NotNull()
                .WithMessage("Reorder data is required");

            RuleFor(x => x.Data.Rules)
                .NotEmpty()
                .WithMessage("At least one rule must be provided");

            RuleForEach(x => x.Data.Rules)
                .ChildRules(rule =>
                {
                    rule.RuleFor(r => r.RuleId)
                        .NotEmpty()
                        .WithMessage("Rule ID is required");

                    rule.RuleFor(r => r.Priority)
                        .GreaterThan(0)
                        .WithMessage("Priority must be greater than 0");
                });

            // Ensure no duplicate rule IDs
            RuleFor(x => x.Data.Rules)
                .Must(rules => rules.Select(r => r.RuleId).Distinct().Count() == rules.Count)
                .WithMessage("Duplicate rule IDs are not allowed");

            // Ensure no duplicate priorities
            RuleFor(x => x.Data.Rules)
                .Must(rules => rules.Select(r => r.Priority).Distinct().Count() == rules.Count)
                .WithMessage("Duplicate priorities are not allowed");
        }
    }

}
