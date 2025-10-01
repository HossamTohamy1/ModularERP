using FluentValidation;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Validators
{
    public class CreateWarehouseDtoValidator : AbstractValidator<CreateWarehouseDto>
    {
        public CreateWarehouseDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Warehouse name is required")
                .MaximumLength(100).WithMessage("Warehouse name cannot exceed 100 characters");

            RuleFor(x => x.ShippingAddress)
                .MaximumLength(255).WithMessage("Shipping address cannot exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.ShippingAddress));

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Warehouse status is required")
                .Must(s => s == "Active" || s == "Inactive")
                .WithMessage("Warehouse status must be Active or Inactive");

            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("Company ID is required");
        }
    }
}