using FluentValidation;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Line;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Validators.Validators_StockTaking_Line
{
    public class CreateStocktakingLineDtoValidator : AbstractValidator<CreateStocktakingLineDto>
    {
        public CreateStocktakingLineDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required");

            RuleFor(x => x.PhysicalQty)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Physical quantity cannot be negative");

            RuleFor(x => x.Note)
                .MaximumLength(1000)
                .WithMessage("Note cannot exceed 1000 characters");

            RuleFor(x => x.ImagePath)
                .MaximumLength(500)
                .WithMessage("Image path cannot exceed 500 characters");
        }
    }

}
