using FluentValidation;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Line;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Validators.Validators_StockTaking_Line
{
    public class CreateBulkStocktakingLinesValidator : AbstractValidator<CreateBulkStocktakingLinesCommand>
    {
        public CreateBulkStocktakingLinesValidator()
        {
            RuleFor(x => x.StocktakingId)
                .NotEmpty()
                .WithMessage("Stocktaking ID is required");

            RuleFor(x => x.Lines)
                .NotEmpty()
                .WithMessage("At least one line is required")
                .Must(lines => lines.Count <= 1000)
                .WithMessage("Cannot create more than 1000 lines at once");

            RuleForEach(x => x.Lines)
                .SetValidator(new CreateStocktakingLineDtoValidator());
        }
    }
}
