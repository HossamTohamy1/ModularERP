using FluentValidation;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commands_StockTraking_WorkFlow;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Validators.Validators_StockTraking_WorkFlow
{
    public class StartStocktakingCommandValidator : AbstractValidator<StartStocktakingCommand>
    {
        public StartStocktakingCommandValidator()
        {
            RuleFor(x => x.StocktakingId)
                .NotEmpty()
                .WithMessage("Stocktaking ID is required");

            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("Company ID is required");

            //RuleFor(x => x.UserId)
            //    .NotEmpty()
            //    .WithMessage("User ID is required");
        }
    }

}
