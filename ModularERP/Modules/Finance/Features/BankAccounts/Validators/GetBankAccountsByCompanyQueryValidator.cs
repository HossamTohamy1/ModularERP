using FluentValidation;
using ModularERP.Modules.Finance.Features.BankAccounts.Queries;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Validators
{
    public class GetBankAccountsByCompanyQueryValidator : AbstractValidator<GetBankAccountsByCompanyQuery>
    {
        public GetBankAccountsByCompanyQueryValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("Company ID is required");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page Number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(100)
                .WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.Search)
                .MaximumLength(100)
                .WithMessage("Search term cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Search));
        }
    }
}
