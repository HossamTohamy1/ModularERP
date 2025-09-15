using FluentValidation;
using ModularERP.Modules.Finance.Features.Treasuries.Commands;

namespace ModularERP.Modules.Finance.Features.Treasuries.Validators
{
    public class DeleteTreasuryCommandValidator : AbstractValidator<DeleteTreasuryCommand>
    {
        public DeleteTreasuryCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Treasury ID is required");
        }
    }
}
