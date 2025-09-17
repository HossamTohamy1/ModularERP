using FluentValidation;
using ModularERP.Modules.Finance.Features.BankAccounts.DTO;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Validators
{
    public class CreateBankAccountDtoValidator : AbstractValidator<CreateBankAccountDto>
    {
        public CreateBankAccountDtoValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("Company ID is required");

            RuleFor(x => x.BankName)
                .NotEmpty()
                .WithMessage("Bank name is required")
                .MaximumLength(100)
                .WithMessage("Bank name cannot exceed 100 characters");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Account name is required")
                .MaximumLength(100)
                .WithMessage("Account name cannot exceed 100 characters");

            RuleFor(x => x.AccountNumber)
                .NotEmpty()
                .WithMessage("Account number is required")
                .MaximumLength(50)
                .WithMessage("Account number cannot exceed 50 characters");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty()
                .WithMessage("Currency code is required")
                .Length(3)
                .WithMessage("Currency code must be exactly 3 characters")
                .Matches("^[A-Z]{3}$")
                .WithMessage("Currency code must contain only uppercase letters");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid bank account status");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.DepositAcl)
                .NotNull()
                .WithMessage("Deposit ACL is required");

            RuleFor(x => x.WithdrawAcl)
                .NotNull()
                .WithMessage("Withdraw ACL is required");
        }
    }
}
