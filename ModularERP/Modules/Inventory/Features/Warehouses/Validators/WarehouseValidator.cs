using FluentValidation;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Validators
{
    public class WarehouseValidator : AbstractValidator<Warehouse>
    {
        public WarehouseValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Warehouse name is required")
                .MaximumLength(100).WithMessage("Warehouse name cannot exceed 100 characters");

            RuleFor(x => x.ShippingAddress)
                .MaximumLength(255).WithMessage("Shipping address cannot exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.ShippingAddress));

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid warehouse status");

            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("Company ID is required");
        }
    }
}
