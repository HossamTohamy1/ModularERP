using FluentValidation;
using ModularERP.Modules.Inventory.Features.TaxManagement.Commends;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Validators
{
    public class AddTaxComponentToProfileValidator : AbstractValidator<AddTaxComponentToProfileCommand>
    {
        public AddTaxComponentToProfileValidator()
        {
            RuleFor(x => x.TaxProfileId)
                .NotEmpty().WithMessage("Tax Profile Id is required");

            RuleFor(x => x.TaxComponentId)
                .NotEmpty().WithMessage("Tax Component Id is required");

            RuleFor(x => x.Priority)
                .InclusiveBetween(1, 100).WithMessage("Priority must be between 1 and 100");
        }
    }
}
