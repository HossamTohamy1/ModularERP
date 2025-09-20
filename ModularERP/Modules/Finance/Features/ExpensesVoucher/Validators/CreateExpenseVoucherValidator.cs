using FluentValidation;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO;

namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.Validators
{
    public class CreateExpenseVoucherValidator : AbstractValidator<CreateExpenseVoucherDto>
    {
        public CreateExpenseVoucherValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than 0");

            RuleFor(x => x.Date)
                .NotEmpty()
                .WithMessage("Date is required");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description is required")
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.CategoryId)
                .NotEmpty()
                .WithMessage("Category is required");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty()
                .WithMessage("Currency code is required")
                .Length(3)
                .WithMessage("Currency code must be 3 characters");

            RuleFor(x => x.FxRate)
                .GreaterThan(0)
                .WithMessage("FX Rate must be greater than 0");

            // ✅ Source validation
            RuleFor(x => x.Source)
                .NotNull()
                .WithMessage("Source is required");

            RuleFor(x => x.Source.Type)
                .NotEmpty()
                .WithMessage("Source type is required")
                .Must(type => type.Equals("Treasury", StringComparison.OrdinalIgnoreCase) ||
                             type.Equals("BankAccount", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Source type must be Treasury or BankAccount");

            RuleFor(x => x.Source.Id)
                .NotEmpty()
                .WithMessage("Source wallet ID is required");

            // ✅ Counterparty validation (optional)
            When(x => x.Counterparty != null, () => {
                RuleFor(x => x.Counterparty.Type)
                    .NotEmpty()
                    .WithMessage("Counterparty type is required when counterparty is specified")
                    .Must(type => type.Equals("Vendor", StringComparison.OrdinalIgnoreCase) ||
                                 type.Equals("Customer", StringComparison.OrdinalIgnoreCase) ||
                                 type.Equals("Other", StringComparison.OrdinalIgnoreCase))
                    .WithMessage("Counterparty type must be Vendor, Customer, or Other");

                RuleFor(x => x.Counterparty.Id)
                    .NotEmpty()
                    .WithMessage("Counterparty ID is required when counterparty is specified");
            });

            // ✅ Tax lines validation (optional)
            RuleForEach(x => x.TaxLines)
                .SetValidator(new TaxLineDtoValidator());
        }
    }

    public class TaxLineDtoValidator : AbstractValidator<TaxLineDto>
    {
        public TaxLineDtoValidator()
        {
            RuleFor(x => x.TaxId)
                .NotEmpty()
                .WithMessage("Tax ID is required");

            RuleFor(x => x.BaseAmount)
                .GreaterThan(0)
                .WithMessage("Base amount must be greater than 0");

            RuleFor(x => x.TaxAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Tax amount must be non-negative");
        }
    }
}