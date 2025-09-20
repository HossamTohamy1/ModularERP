using FluentValidation;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Validators;
using ModularERP.Modules.Finance.Features.IncomesVoucher.DTO;

namespace ModularERP.Modules.Finance.Features.IncomesVoucher.Validators
{
    public class CreateIncomeVoucherValidator : AbstractValidator<CreateIncomeVoucherDto>
    {
        public CreateIncomeVoucherValidator()
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

            RuleFor(x => x.Source)
                .NotNull()
                .WithMessage("Source wallet is required");

            RuleFor(x => x.Source.Type)
                .Must(type => type == "Treasury" || type == "BankAccount")
                .WithMessage("Source type must be Treasury or BankAccount");

            RuleFor(x => x.Source.Id)
                .NotEmpty()
                .WithMessage("Source wallet ID is required");

            RuleFor(x => x.FxRate)
                .GreaterThan(0)
                .WithMessage("FX Rate must be greater than 0");

        }
    }
    public class TaxLineValidator : AbstractValidator<TaxLineDto>
    {
        public TaxLineValidator()
        {
            RuleFor(x => x.TaxId)
                .NotEmpty()
                .WithMessage("Tax ID is required");

            RuleFor(x => x.BaseAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Base amount must be greater than or equal to 0");

            RuleFor(x => x.TaxAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Tax amount must be greater than or equal to 0");
        }
    }
}