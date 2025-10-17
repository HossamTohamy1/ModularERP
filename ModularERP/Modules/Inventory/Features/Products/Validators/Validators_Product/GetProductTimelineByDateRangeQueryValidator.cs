using FluentValidation;
using ModularERP.Modules.Inventory.Features.Products.Qeuries.Qeuries_Product;

namespace ModularERP.Modules.Inventory.Features.Products.Validators.Validators_Product
{
    public class GetProductTimelineByDateRangeQueryValidator : AbstractValidator<GetProductTimelineByDateRangeQuery>
    {
        public GetProductTimelineByDateRangeQueryValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be greater than or equal to start date");
        }
    }
}
