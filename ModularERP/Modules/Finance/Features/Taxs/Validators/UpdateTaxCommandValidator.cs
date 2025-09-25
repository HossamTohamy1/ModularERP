using FluentValidation;
using ModularERP.Modules.Finance.Features.Taxs.Commands;

namespace ModularERP.Modules.Finance.Features.Taxs.Validators
{
    public class UpdateTaxCommandValidator : AbstractValidator<UpdateTaxCommand>
    {
        public UpdateTaxCommandValidator()
        {
            RuleFor(x => x.UpdateTaxDto)
                .NotNull()
                .WithMessage("Tax update data is required")
                .SetValidator(new UpdateTaxDtoValidator());
        }
    }
}
