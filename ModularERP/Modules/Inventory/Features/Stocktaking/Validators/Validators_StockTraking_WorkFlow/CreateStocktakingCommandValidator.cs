using FluentValidation;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commands_StockTraking_WorkFlow;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Validators.Validators_StockTraking_WorkFlow
{
    public class CreateStocktakingCommandValidator : AbstractValidator<CreateStocktakingCommand>
    {
        public CreateStocktakingCommandValidator()
        {
            RuleFor(x => x.WarehouseId)
                .NotEmpty()
                .WithMessage("Warehouse ID is required");

            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("Company ID is required");

            RuleFor(x => x.Number)
                .NotEmpty()
                .WithMessage("Number is required")
                .MaximumLength(50)
                .WithMessage("Number cannot exceed 50 characters");

            RuleFor(x => x.DateTime)
                .NotEmpty()
                .WithMessage("Date and time is required")
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Date cannot be in the future");

            RuleFor(x => x.CreatedByUserId)
                .NotEmpty()
                .WithMessage("Created by user ID is required");
        }
    }
}
