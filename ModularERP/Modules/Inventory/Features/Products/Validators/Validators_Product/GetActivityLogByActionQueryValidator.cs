using FluentValidation;
using ModularERP.Modules.Inventory.Features.Products.Qeuries.Qeuries_Product;

namespace ModularERP.Modules.Inventory.Features.Products.Validators.Validators_Product
{
    public class GetActivityLogByActionQueryValidator : AbstractValidator<GetActivityLogByActionQuery>
    {
        public GetActivityLogByActionQueryValidator()
        {
            RuleFor(x => x.ActionType)
                .NotEmpty().WithMessage("Action type is required")
                .MaximumLength(100).WithMessage("Action type cannot exceed 100 characters");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");
        }
    }
}
