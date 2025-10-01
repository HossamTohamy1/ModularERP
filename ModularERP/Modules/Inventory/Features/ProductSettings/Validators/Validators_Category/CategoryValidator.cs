using FluentValidation;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_Category
{
    public class CategoryValidator : AbstractValidator<Category>
    {
        public CategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required")
                .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            // Prevent circular reference (Category cannot be its own parent)
            RuleFor(x => x.ParentCategoryId)
                .Must((category, parentId) => parentId != category.Id)
                .WithMessage("Category cannot be its own parent")
                .When(x => x.ParentCategoryId.HasValue);
        }
    }
}
