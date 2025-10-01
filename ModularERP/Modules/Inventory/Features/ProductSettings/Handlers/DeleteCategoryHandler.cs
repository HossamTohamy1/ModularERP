using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers
{
    public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Category> _categoryRepo;
        private readonly IGeneralRepository<CategoryAttachment> _attachmentRepo;

        public DeleteCategoryHandler(
            IGeneralRepository<Category> categoryRepo,
            IGeneralRepository<CategoryAttachment> attachmentRepo)
        {
            _categoryRepo = categoryRepo;
            _attachmentRepo = attachmentRepo;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            DeleteCategoryCommand request,
            CancellationToken cancellationToken)
        {
            var category = await _categoryRepo.GetByID(request.Id);

            if (category == null)
                throw new NotFoundException("Category not found", FinanceErrorCode.NotFound);

            // Check if category has subcategories
            var hasSubCategories = await _categoryRepo
                .Get(c => c.ParentCategoryId == request.Id)
                .AnyAsync(cancellationToken);

            if (hasSubCategories)
            {
                throw new BusinessLogicException(
                    "Cannot delete category with subcategories. Please delete or reassign subcategories first.",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            // Delete all attachments (physical files and DB records)
            var attachments = await _attachmentRepo
                .Get(a => a.CategoryId == request.Id)
                .ToListAsync(cancellationToken);

            foreach (var attachment in attachments)
            {
                DeletePhysicalFile(attachment.FilePath);
                await _attachmentRepo.Delete(attachment.Id);
            }

            // Soft delete category
            await _categoryRepo.Delete(request.Id);
            await _categoryRepo.SaveChanges();

            return ResponseViewModel<bool>.Success(true, "Category deleted successfully");
        }

        private void DeletePhysicalFile(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (logging mechanism not shown here)
                Console.WriteLine($"Error deleting file {filePath}: {ex.Message}");
            }
        }
    }
}
