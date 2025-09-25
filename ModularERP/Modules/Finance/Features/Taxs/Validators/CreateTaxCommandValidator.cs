using FluentValidation;
using ModularERP.Modules.Finance.Features.Taxs.Commands;
using ModularERP.Modules.Finance.Features.Taxs.DTO;

namespace ModularERP.Modules.Finance.Features.Taxs.Validators
{
    public class CreateTaxCommandValidator : AbstractValidator<CreateTaxCommand>
    {
        public CreateTaxCommandValidator()
        {
            RuleFor(x => x.CreateTaxDto)
                .NotNull()
                .WithMessage("Tax data is required")
                .SetValidator(new CreateTaxDtoValidator());
        }
    }
}

