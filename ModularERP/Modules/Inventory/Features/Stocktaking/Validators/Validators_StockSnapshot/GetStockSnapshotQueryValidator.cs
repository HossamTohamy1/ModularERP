using FluentValidation;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockSnapshot;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Validators.Validators_StockSnapshot
{
    public class GetStockSnapshotQueryValidator : AbstractValidator<GetStockSnapshotQuery>
    {
        public GetStockSnapshotQueryValidator()
        {
            RuleFor(x => x.StocktakingId)
                .NotEmpty()
                .WithMessage("Stocktaking ID is required")
                .NotEqual(Guid.Empty)
                .WithMessage("Stocktaking ID cannot be empty");
        }
    }
}