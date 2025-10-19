using FluentValidation;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Line;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Validators.Validators_StockTaking_Line
{
    public class DeleteStocktakingLineValidator : AbstractValidator<DeleteStocktakingLineCommand>
    {
        public DeleteStocktakingLineValidator()
        {
            RuleFor(x => x.StocktakingId)
                .NotEmpty()
                .WithMessage("Stocktaking ID is required");

            RuleFor(x => x.LineId)
                .NotEmpty()
                .WithMessage("Line ID is required");
        }
    }
}
