using FluentValidation;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO.DTO_WarehouseStock;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Validators
{
    public class CreateWarehouseStockValidator : AbstractValidator<CreateWarehouseStockDto>
    {
        public CreateWarehouseStockValidator()
        {
            RuleFor(x => x.WarehouseId)
                .NotEmpty().WithMessage("Warehouse is required");

            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product is required");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity must be greater than or equal to 0");

            RuleFor(x => x.ReservedQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Reserved quantity must be greater than or equal to 0")
                .When(x => x.ReservedQuantity.HasValue);

            RuleFor(x => x.ReservedQuantity)
                .LessThanOrEqualTo(x => x.Quantity)
                .WithMessage("Reserved quantity cannot exceed total quantity")
                .When(x => x.ReservedQuantity.HasValue);

            RuleFor(x => x.MinStockLevel)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum stock level must be greater than or equal to 0")
                .When(x => x.MinStockLevel.HasValue);

            RuleFor(x => x.MaxStockLevel)
                .GreaterThanOrEqualTo(x => x.MinStockLevel ?? 0)
                .WithMessage("Maximum stock level must be greater than minimum stock level")
                .When(x => x.MaxStockLevel.HasValue && x.MinStockLevel.HasValue);
        }
    }

}
