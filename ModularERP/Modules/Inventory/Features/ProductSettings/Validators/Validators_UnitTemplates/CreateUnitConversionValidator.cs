using FluentValidation;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_UnitTemplates;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_UnitTemplates
{
    public class CreateUnitConversionValidator : AbstractValidator<CreateUnitConversionDto>
    {
        public CreateUnitConversionValidator()
        {
            RuleFor(x => x.UnitName)
                .NotEmpty().WithMessage("Unit name is required")
                .MaximumLength(50).WithMessage("Unit name cannot exceed 50 characters");

            RuleFor(x => x.ShortName)
                .NotEmpty().WithMessage("Short name is required")
                .MaximumLength(10).WithMessage("Short name cannot exceed 10 characters");

            RuleFor(x => x.Factor)
                .GreaterThan(0).WithMessage("Factor must be greater than zero");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order cannot be negative");
        }
    }
}
