using FluentValidation;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commands_StockTraking_WorkFlow;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Validators.Validators_StockTraking_WorkFlow
{
    public class ReviewStocktakingCommandValidator : AbstractValidator<ReviewStocktakingCommand>
    {
        public ReviewStocktakingCommandValidator()
        {
            RuleFor(x => x.StocktakingId)
                .NotEmpty()
                .WithMessage("Stocktaking ID is required");

            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("Company ID is required");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");
        }
    }
}
