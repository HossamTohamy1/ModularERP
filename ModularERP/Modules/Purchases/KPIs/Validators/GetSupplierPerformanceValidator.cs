using FluentValidation;
using ModularERP.Modules.Purchases.KPIs.Qeuries;

namespace ModularERP.Modules.Purchases.KPIs.Validators
{
    public class GetSupplierPerformanceValidator : AbstractValidator<GetSupplierPerformanceQuery>
    {
        public GetSupplierPerformanceValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("Company ID is required");

            RuleFor(x => x.TopCount)
                .GreaterThan(0)
                .WithMessage("Top count must be greater than 0")
                .LessThanOrEqualTo(100)
                .WithMessage("Top count cannot exceed 100");

            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate ?? DateTime.UtcNow)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("Start date must be before or equal to end date");

            RuleFor(x => x.EndDate)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .When(x => x.EndDate.HasValue)
                .WithMessage("End date cannot be in the future");
        }
    }
}