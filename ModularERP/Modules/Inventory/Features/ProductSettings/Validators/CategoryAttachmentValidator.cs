using FluentValidation;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Validators
{
    public class CategoryAttachmentValidator : AbstractValidator<CategoryAttachment>
    {
        public CategoryAttachmentValidator()
        {
            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category ID is required");

            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("File name is required")
                .MaximumLength(255).WithMessage("File name cannot exceed 255 characters");

            RuleFor(x => x.FilePath)
                .NotEmpty().WithMessage("File path is required")
                .MaximumLength(500).WithMessage("File path cannot exceed 500 characters");

            RuleFor(x => x.FileType)
                .MaximumLength(100).WithMessage("File type cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.FileType));

            RuleFor(x => x.FileSize)
                .GreaterThan(0).WithMessage("File size must be greater than 0")
                .LessThanOrEqualTo(10 * 1024 * 1024).WithMessage("File size cannot exceed 10 MB");
        }
    }
}
