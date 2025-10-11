using FluentValidation;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Validators
{
    public class UpdateStockTransactionValidator : AbstractValidator<UpdateStockTransactionDto>
    {
        public UpdateStockTransactionValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Transaction ID is required");

            RuleFor(x => x.TransactionType)
                .IsInEnum()
                .WithMessage("Invalid transaction type");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero")
                .LessThanOrEqualTo(999999.999m)
                .WithMessage("Quantity cannot exceed 999,999.999");

            RuleFor(x => x.UnitCost)
                .GreaterThanOrEqualTo(0)
                .When(x => x.UnitCost.HasValue)
                .WithMessage("Unit cost cannot be negative");

            RuleFor(x => x.ReferenceType)
                .MaximumLength(50)
                .WithMessage("Reference type cannot exceed 50 characters");
        }
    }

}
