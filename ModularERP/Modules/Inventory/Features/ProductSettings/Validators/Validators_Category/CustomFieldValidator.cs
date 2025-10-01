using FluentValidation;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_Category
{
    public class CustomFieldValidator : AbstractValidator<CustomField>
    {
        public CustomFieldValidator()
        {
            RuleFor(x => x.FieldName)
                .NotEmpty().WithMessage("Field name is required")
                .MaximumLength(100).WithMessage("Field name cannot exceed 100 characters")
                .Matches("^[a-zA-Z0-9_]+$").WithMessage("Field name can only contain letters, numbers, and underscores");

            RuleFor(x => x.FieldLabel)
                .NotEmpty().WithMessage("Field label is required")
                .MaximumLength(100).WithMessage("Field label cannot exceed 100 characters");

            RuleFor(x => x.FieldType)
                .IsInEnum().WithMessage("Invalid field type");

            RuleFor(x => x.DefaultValue)
                .MaximumLength(1000).WithMessage("Default value cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.DefaultValue));

            RuleFor(x => x.Options)
                .NotEmpty().WithMessage("Options are required for dropdown and multi-select fields")
                .MaximumLength(2000).WithMessage("Options cannot exceed 2000 characters")
                .When(x => x.FieldType == CustomFieldType.Dropdown || x.FieldType == CustomFieldType.MultiSelect);

            RuleFor(x => x.ValidationRules)
                .MaximumLength(500).WithMessage("Validation rules cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.ValidationRules));

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order cannot be negative");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid custom field status");

            RuleFor(x => x.HelpText)
                .MaximumLength(500).WithMessage("Help text cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.HelpText));
        }
    }
}
