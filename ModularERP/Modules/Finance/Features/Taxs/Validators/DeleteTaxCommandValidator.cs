using FluentValidation;
using ModularERP.Modules.Finance.Features.Taxs.Commands;

namespace ModularERP.Modules.Finance.Features.Taxs.Validators
{
    public class DeleteTaxCommandValidator : AbstractValidator<DeleteTaxCommand>
    {
        public DeleteTaxCommandValidator()
        {
            RuleFor(x => x.TaxId)
                .NotEmpty()
                .WithMessage("Tax ID is required for deletion");
        }
    }
}
