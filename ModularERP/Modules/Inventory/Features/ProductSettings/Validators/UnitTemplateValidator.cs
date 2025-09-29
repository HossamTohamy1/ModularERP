using FluentValidation;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Validators
{
    public class UnitTemplateValidator : AbstractValidator<UnitTemplate>
    {
        public UnitTemplateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Unit template name is required")
                .MaximumLength(100).WithMessage("Unit template name cannot exceed 100 characters");

            RuleFor(x => x.BaseUnitName)
                .NotEmpty().WithMessage("Base unit name is required")
                .MaximumLength(50).WithMessage("Base unit name cannot exceed 50 characters");

            RuleFor(x => x.BaseUnitShortName)
                .NotEmpty().WithMessage("Base unit short name is required")
                .MaximumLength(10).WithMessage("Base unit short name cannot exceed 10 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid unit template status");
        }
    }
}
