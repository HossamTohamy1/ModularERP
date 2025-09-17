using FluentValidation;
using ModularERP.Modules.Finance.Features.BankAccounts.Commands;
using ModularERP.Modules.Finance.Features.BankAccounts.DTO;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Validators
{
    public class UpdateBankAccountCommandValidator : AbstractValidator<UpdateBankAccountCommand>
    {
        public UpdateBankAccountCommandValidator()
        {
            RuleFor(x => x.BankAccount)
                .NotNull()
                .WithMessage("Bank account data is required")
                .SetValidator(new UpdateBankAccountDtoValidator());
        }
    }
}

