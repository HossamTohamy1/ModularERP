using FluentValidation;
using ModularERP.Modules.Inventory.Features.Products.Qeuries.Qeuries_ProductStats;

namespace ModularERP.Modules.Inventory.Features.Products.Validators.Validators_ProductStats
{
    public class GetProductStatsQueryValidator : AbstractValidator<GetProductStatsQuery>
    {
        public GetProductStatsQueryValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required");

            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("Company ID is required");
        }
    }
}
