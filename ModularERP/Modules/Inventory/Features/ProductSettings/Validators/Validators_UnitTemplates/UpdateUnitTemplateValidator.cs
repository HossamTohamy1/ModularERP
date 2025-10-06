using FluentValidation;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_UnitTemplates;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_UnitTemplates
{
    public class UpdateUnitTemplateValidator : AbstractValidator<UpdateUnitTemplateDto>
    {
        public UpdateUnitTemplateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

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
                .IsInEnum().WithMessage("Invalid status value");
        }
    }
}
