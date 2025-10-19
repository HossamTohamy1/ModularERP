using FluentValidation;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTraking_WorkFlow;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Validators.Validators_StockTraking_WorkFlow
{
    public class GetAllStocktakingsQueryValidator : AbstractValidator<GetAllStocktakingsQuery>
    {
        public GetAllStocktakingsQueryValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("Company ID is required");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100)
                .WithMessage("Page size cannot exceed 100");

            RuleFor(x => x)
                .Custom((query, context) =>
                {
                    if (query.FromDate.HasValue && query.ToDate.HasValue && query.FromDate > query.ToDate)
                    {
                        context.AddFailure("FromDate", "From date cannot be greater than to date");
                    }
                });
        }
    }
}

