using FluentValidation;
using ModularERP.Modules.Inventory.Features.StockTransactions.DTO;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Validators
{
    public class CreateStockTransactionValidator : AbstractValidator<CreateStockTransactionDto>
    {
        public CreateStockTransactionValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("Company ID is required");

            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required");

            RuleFor(x => x.WarehouseId)
                .NotEmpty()
                .WithMessage("Warehouse ID is required");

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
