using FluentValidation;
using ModularERP.Modules.Finance.Features.BankAccounts.Queries;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Validators
{
    public class GetBankAccountStatisticsQueryValidator : AbstractValidator<GetBankAccountStatisticsQuery>
    {
        public GetBankAccountStatisticsQueryValidator()
        {
            RuleFor(x => x.FromDate)
                .LessThanOrEqualTo(x => x.ToDate)
                .WithMessage("From date must be less than or equal to To date")
                .When(x => x.FromDate.HasValue && x.ToDate.HasValue);

            RuleFor(x => x.ToDate)
                .LessThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("To date cannot be in the future")
                .When(x => x.ToDate.HasValue);
        }
    }
}
