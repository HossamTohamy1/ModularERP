using FluentValidation;
using ModularERP.Modules.Finance.Features.Treasuries.Commands;

namespace ModularERP.Modules.Finance.Features.Treasuries.Validators
{
    public class UpdateTreasuryCommandValidator : AbstractValidator<UpdateTreasuryCommand>
    {
        public UpdateTreasuryCommandValidator()
        {
            RuleFor(x => x.Treasury)
                .NotNull()
                .WithMessage("Treasury data is required")
                .SetValidator(new UpdateTreasuryDtoValidator());
        }
    }
}
