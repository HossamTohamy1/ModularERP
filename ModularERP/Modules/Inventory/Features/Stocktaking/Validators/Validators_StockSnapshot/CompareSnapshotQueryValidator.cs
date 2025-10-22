using FluentValidation;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockSnapshot;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Validators.Validators_StockSnapshot
{
    public class CompareSnapshotQueryValidator : AbstractValidator<CompareSnapshotQuery>
    {
        public CompareSnapshotQueryValidator()
        {
            RuleFor(x => x.StocktakingId)
                .NotEmpty()
                .WithMessage("Stocktaking ID is required")
                .NotEqual(Guid.Empty)
                .WithMessage("Stocktaking ID cannot be empty");

            RuleFor(x => x.DriftThreshold)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Drift threshold must be greater than or equal to 0")
                .LessThanOrEqualTo(100)
                .WithMessage("Drift threshold cannot exceed 100%");
        }
    }
}
