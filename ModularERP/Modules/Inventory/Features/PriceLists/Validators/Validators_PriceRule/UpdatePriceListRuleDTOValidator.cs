using FluentValidation;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceRule;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceRule
{
    public class UpdatePriceListRuleDTOValidator : AbstractValidator<UpdatePriceListRuleDTO>
    {
        public UpdatePriceListRuleDTOValidator()
        {
            RuleFor(x => x.RuleType)
                .IsInEnum()
                .WithMessage("Invalid rule type");

            RuleFor(x => x.Value)
                .GreaterThan(0)
                .When(x => x.Value.HasValue)
                .WithMessage("Value must be greater than 0");

            RuleFor(x => x.Priority)
                .GreaterThan(0)
                .WithMessage("Priority must be greater than 0");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("End date must be after start date");

            RuleFor(x => x.Value)
                .NotNull()
                .When(x => x.RuleType == PriceRuleType.Markup ||
                          x.RuleType == PriceRuleType.Margin ||
                          x.RuleType == PriceRuleType.FixedAdjustment)
                .WithMessage("Value is required for this rule type");
        }
    }
}
