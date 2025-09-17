using FluentValidation;
using ModularERP.Modules.Finance.Features.BankAccounts.Commands;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Validators
{
    public class CreateBankAccountCommandValidator : AbstractValidator<CreateBankAccountCommand>
    {
        public CreateBankAccountCommandValidator()
        {
            RuleFor(x => x.BankAccount)
                .NotNull()
                .WithMessage("Bank account data is required")
                .SetValidator(new CreateBankAccountDtoValidator());
        }
    }
}

