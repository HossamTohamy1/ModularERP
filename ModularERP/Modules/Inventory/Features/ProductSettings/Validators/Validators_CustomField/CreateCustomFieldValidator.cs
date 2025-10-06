using FluentValidation;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Command_Custom;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_CustomField
{
    public class CreateCustomFieldValidator : AbstractValidator<CreateCustomFieldCommand>
    {
        public CreateCustomFieldValidator()
        {
            RuleFor(x => x.FieldName)
                .NotEmpty().WithMessage("Field name is required")
                .MaximumLength(100).WithMessage("Field name cannot exceed 100 characters");

            RuleFor(x => x.FieldLabel)
                .NotEmpty().WithMessage("Field label is required")
                .MaximumLength(100).WithMessage("Field label cannot exceed 100 characters");

            RuleFor(x => x.FieldType)
                .IsInEnum().WithMessage("Invalid field type");

            RuleFor(x => x.DefaultValue)
                .MaximumLength(1000).WithMessage("Default value cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.DefaultValue));

            RuleFor(x => x.Options)
                .MaximumLength(2000).WithMessage("Options cannot exceed 2000 characters")
                .When(x => !string.IsNullOrEmpty(x.Options));

            RuleFor(x => x.ValidationRules)
                .MaximumLength(500).WithMessage("Validation rules cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.ValidationRules));

            RuleFor(x => x.HelpText)
                .MaximumLength(500).WithMessage("Help text cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.HelpText));

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0");
        }
    }
}
