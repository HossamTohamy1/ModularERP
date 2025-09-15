using FluentValidation;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;

namespace ModularERP.Modules.Finance.Features.Treasuries.Validators
{
    public class UpdateTreasuryDtoValidator : AbstractValidator<UpdateTreasuryDto>
    {
        public UpdateTreasuryDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Treasury ID is required");

            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("Company ID is required");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Treasury name is required")
                .MaximumLength(100)
                .WithMessage("Treasury name cannot exceed 100 characters");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty()
                .WithMessage("Currency code is required")
                .Length(3)
                .WithMessage("Currency code must be exactly 3 characters")
                .Matches("^[A-Z]{3}$")
                .WithMessage("Currency code must contain only uppercase letters");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid treasury status");

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
