using FluentValidation;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Line;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Validators.Validators_StockTaking_Line
{
    public class AddAllWarehouseProductsValidator : AbstractValidator<AddAllWarehouseProductsCommand>
    {
        public AddAllWarehouseProductsValidator()
        {
            RuleFor(x => x.StocktakingId)
                .NotEmpty()
                .WithMessage("Stocktaking ID is required");
        }
    }
}
