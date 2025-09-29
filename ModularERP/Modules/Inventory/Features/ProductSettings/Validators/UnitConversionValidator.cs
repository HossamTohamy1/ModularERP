using FluentValidation;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Validators
{
    public class UnitConversionValidator : AbstractValidator<UnitConversion>
    {
        public UnitConversionValidator()
        {
            RuleFor(x => x.UnitTemplateId)
                .NotEmpty().WithMessage("Unit template ID is required");

            RuleFor(x => x.UnitName)
                .NotEmpty().WithMessage("Unit name is required")
                .MaximumLength(50).WithMessage("Unit name cannot exceed 50 characters");

            RuleFor(x => x.ShortName)
                .NotEmpty().WithMessage("Short name is required")
                .MaximumLength(10).WithMessage("Short name cannot exceed 10 characters");

            RuleFor(x => x.Factor)
                .GreaterThan(0).WithMessage("Conversion factor must be greater than 0");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order cannot be negative");
        }
    }

}
