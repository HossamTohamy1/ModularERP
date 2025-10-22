using FluentValidation;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockSnapshot;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Validators.Validators_StockSnapshot
{
    public class RefreshSnapshotCommandValidator : AbstractValidator<RefreshSnapshotCommand>
    {
        public RefreshSnapshotCommandValidator()
        {
            RuleFor(x => x.StocktakingId)
                .NotEmpty()
                .WithMessage("Stocktaking ID is required")
                .NotEqual(Guid.Empty)
                .WithMessage("Stocktaking ID cannot be empty");
        }
    }
}
