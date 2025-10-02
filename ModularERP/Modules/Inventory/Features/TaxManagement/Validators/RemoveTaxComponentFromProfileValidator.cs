using FluentValidation;
using ModularERP.Modules.Inventory.Features.TaxManagement.Commends;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Validators
{
    public class RemoveTaxComponentFromProfileValidator : AbstractValidator<RemoveTaxComponentFromProfileCommand>
    {
        public RemoveTaxComponentFromProfileValidator()
        {
            RuleFor(x => x.TaxProfileId)
                .NotEmpty().WithMessage("Tax Profile Id is required");

            RuleFor(x => x.TaxComponentId)
                .NotEmpty().WithMessage("Tax Component Id is required");
        }
    }
}
