using FluentValidation;
using ModularERP.Modules.Finance.Features.BankAccounts.Commands;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Validators
{
    public class DeleteBankAccountCommandValidator : AbstractValidator<DeleteBankAccountCommand>
    {
        public DeleteBankAccountCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Bank account ID is required");
        }
    }
}