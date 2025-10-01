using FluentValidation;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Validators
{
    public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDto>
    {
        private readonly IGeneralRepository<Category> _categoryRepository;

        public UpdateCategoryValidator(IGeneralRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Category ID is required")
                .MustAsync(CategoryExists).WithMessage("Category not found");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
                .MustAsync(BeUniqueNameForUpdate).WithMessage("Category name already exists");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.ParentCategoryId)
                .MustAsync(ParentCategoryExists)
                .When(x => x.ParentCategoryId.HasValue)
                .WithMessage("Parent category does not exist")
                .MustAsync(NotBeCircularReference)
                .When(x => x.ParentCategoryId.HasValue)
                .WithMessage("Cannot set category as its own parent or child");

            RuleFor(x => x.NewAttachments)
                .Must(ValidateAttachments)
                .When(x => x.NewAttachments != null && x.NewAttachments.Any())
                .WithMessage("Invalid attachments. Max 5MB per file, allowed types: jpg, jpeg, png, pdf, doc, docx");
        }

        private async Task<bool> CategoryExists(Guid id, CancellationToken cancellationToken)
        {
            return await _categoryRepository.AnyAsync(c => c.Id == id);
        }

        private async Task<bool> BeUniqueNameForUpdate(UpdateCategoryDto dto, string name, CancellationToken cancellationToken)
        {
            return !await _categoryRepository.AnyAsync(c => c.Name.ToLower() == name.ToLower() && c.Id != dto.Id);
        }

        private async Task<bool> ParentCategoryExists(Guid? parentId, CancellationToken cancellationToken)
        {
            if (!parentId.HasValue) return true;
            return await _categoryRepository.AnyAsync(c => c.Id == parentId.Value);
        }

        private async Task<bool> NotBeCircularReference(UpdateCategoryDto dto, Guid? parentId, CancellationToken cancellationToken)
        {
            if (!parentId.HasValue) return true;

            // Cannot be its own parent
            if (parentId.Value == dto.Id) return false;

            // Check if the parent is a descendant of this category
            var parentCategory = await _categoryRepository.GetByID(parentId.Value);
            if (parentCategory == null) return true;

            var currentParentId = parentCategory.ParentCategoryId;
            while (currentParentId.HasValue)
            {
                if (currentParentId.Value == dto.Id) return false;

                var nextParent = await _categoryRepository.GetByID(currentParentId.Value);
                currentParentId = nextParent?.ParentCategoryId;
            }

            return true;
        }

        private bool ValidateAttachments(List<Microsoft.AspNetCore.Http.IFormFile>? attachments)
        {
            if (attachments == null || !attachments.Any()) return true;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };
            var maxFileSize = 5 * 1024 * 1024; // 5MB

            return attachments.All(file =>
            {
                var extension = Path.GetExtension(file.FileName).ToLower();
                return allowedExtensions.Contains(extension) && file.Length <= maxFileSize;
            });
        }
    }
}

