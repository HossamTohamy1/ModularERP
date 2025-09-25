using FluentValidation;
using ModularERP.Modules.Finance.Features.Taxs.Commands;

namespace ModularERP.Modules.Finance.Features.Taxs.Validators
{
    public class ToggleTaxStatusCommandValidator : AbstractValidator<ToggleTaxStatusCommand>
    {
        public ToggleTaxStatusCommandValidator()
        {
            RuleFor(x => x.TaxId)
                .NotEmpty()
                .WithMessage("Tax ID is required to toggle status");
        }
    }
}
