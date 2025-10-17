using FluentValidation;
using ModularERP.Modules.Inventory.Features.Products.Qeuries.Qeuries_Product;

namespace ModularERP.Modules.Inventory.Features.Products.Validators.Validators_Product
{
    public class GetActivityLogQueryValidator : AbstractValidator<GetActivityLogQuery>
    {
        public GetActivityLogQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");
        }
    }
}
