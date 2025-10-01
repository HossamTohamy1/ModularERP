using FluentValidation;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Validators
{
    public class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
    {
        private readonly IGeneralRepository<Category> _categoryRepository;

        public CreateCategoryValidator(IGeneralRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
                .MustAsync(BeUniqueName).WithMessage("Category name already exists");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.ParentCategoryId)
                .MustAsync(ParentCategoryExists)
                .When(x => x.ParentCategoryId.HasValue)
                .WithMessage("Parent category does not exist");

            RuleFor(x => x.Attachments)
                .Must(ValidateAttachments)
                .When(x => x.Attachments != null && x.Attachments.Any())
                .WithMessage("Invalid attachments. Max 5MB per file, allowed types: jpg, jpeg, png, pdf, doc, docx");
        }

        private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
        {
            return !await _categoryRepository.AnyAsync(c => c.Name.ToLower() == name.ToLower());
        }

        private async Task<bool> ParentCategoryExists(Guid? parentId, CancellationToken cancellationToken)
        {
            if (!parentId.HasValue) return true;
            return await _categoryRepository.AnyAsync(c => c.Id == parentId.Value);
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
