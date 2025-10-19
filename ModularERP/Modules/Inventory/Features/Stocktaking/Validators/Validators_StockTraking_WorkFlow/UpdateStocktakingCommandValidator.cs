using FluentValidation;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Validators.Validators_StockTraking_WorkFlow
{
    public class UpdateStocktakingCommandValidator : AbstractValidator<UpdateStocktakingCommand>
    {
        public UpdateStocktakingCommandValidator()
        {
            RuleFor(x => x.StocktakingId)
                .NotEmpty()
                .WithMessage("Stocktaking ID is required");

            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("Company ID is required");

            RuleFor(x => x.UpdatedByUserId)
                .NotEmpty()
                .WithMessage("Updated by user ID is required");
        }
    }
}
