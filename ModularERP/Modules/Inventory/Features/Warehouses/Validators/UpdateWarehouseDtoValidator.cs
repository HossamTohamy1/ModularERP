using FluentValidation;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Validators
{
    public class UpdateWarehouseDtoValidator : AbstractValidator<UpdateWarehouseDto>
    {
        public UpdateWarehouseDtoValidator()
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
        }
    }
    public class UpdateWarehouseStatusDtoValidator : AbstractValidator<UpdateWarehouseStatusDto>
    {
        public UpdateWarehouseStatusDtoValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .Must(s => s == "Active" || s == "Inactive")
                .WithMessage("Status must be Active or Inactive");
        }
    }
}
