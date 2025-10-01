using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Category;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Category;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handelrs_Category
{
    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, ResponseViewModel<CategoryResponseDto>>
    {
        private readonly IGeneralRepository<Category> _categoryRepo;
        private readonly IGeneralRepository<CategoryAttachment> _attachmentRepo;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContext;
        private readonly string _uploadsPath = "uploads/categories";

        public UpdateCategoryHandler(
            IGeneralRepository<Category> categoryRepo,
            IGeneralRepository<CategoryAttachment> attachmentRepo,
            IMapper mapper,
            IHttpContextAccessor httpContext)
        {
            _categoryRepo = categoryRepo;
            _attachmentRepo = attachmentRepo;
            _mapper = mapper;
            _httpContext = httpContext;
        }

        public async Task<ResponseViewModel<CategoryResponseDto>> Handle(
            UpdateCategoryCommand request,
            CancellationToken cancellationToken)
        {
            // Get existing category
            var existingCategory = await _categoryRepo.GetByIDWithTracking(request.Dto.Id);

            if (existingCategory == null)
                throw new NotFoundException("Category not found", FinanceErrorCode.NotFound);

            // Update properties
            existingCategory.Name = request.Dto.Name;
            existingCategory.Description = request.Dto.Description;
            existingCategory.ParentCategoryId = request.Dto.ParentCategoryId;
            existingCategory.UpdatedAt = DateTime.UtcNow;

            await _categoryRepo.SaveChanges();

            // Handle attachment deletions
            if (request.Dto.AttachmentIdsToDelete != null && request.Dto.AttachmentIdsToDelete.Any())
            {
                foreach (var attachmentId in request.Dto.AttachmentIdsToDelete)
                {
                    var attachment = await _attachmentRepo.GetByID(attachmentId);
                    if (attachment != null && attachment.CategoryId == request.Dto.Id)
                    {
                        DeletePhysicalFile(attachment.FilePath);
                        await _attachmentRepo.Delete(attachmentId);
                    }
                }
                await _attachmentRepo.SaveChanges();
            }

            if (request.Dto.NewAttachments != null && request.Dto.NewAttachments.Any())
            {
                var oldAttachments = await _attachmentRepo
                    .Get(a => a.CategoryId == request.Dto.Id)
                    .ToListAsync(cancellationToken);

                foreach (var oldAttachment in oldAttachments)
                {
                    DeletePhysicalFile(oldAttachment.FilePath);
                    await _attachmentRepo.Delete(oldAttachment.Id);
                }
                await _attachmentRepo.SaveChanges();

                var userId = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53"); 
                var newAttachments = await SaveAttachments(request.Dto.NewAttachments, request.Dto.Id, userId);

                await _attachmentRepo.AddRangeAsync(newAttachments);
                await _attachmentRepo.SaveChanges();
            }


            // Load related data
            var parentCategory = existingCategory.ParentCategoryId.HasValue
                ? await _categoryRepo.GetByID(existingCategory.ParentCategoryId.Value)
                : null;

            var attachmentsList = await _attachmentRepo
                .Get(a => a.CategoryId == request.Dto.Id)
                .ToListAsync(cancellationToken);

            var subCategoriesCount = await _categoryRepo
                .Get(c => c.ParentCategoryId == request.Dto.Id)
                .CountAsync(cancellationToken);

            // Map to Response
            var response = _mapper.Map<CategoryResponseDto>(existingCategory);
            response.ParentCategoryName = parentCategory?.Name;
            response.Attachments = _mapper.Map<List<CategoryAttachmentDto>>(attachmentsList);
            response.SubCategoriesCount = subCategoriesCount;

            return ResponseViewModel<CategoryResponseDto>.Success(
                response,
                "Category updated successfully");
        }

        private async Task<List<CategoryAttachment>> SaveAttachments(
            List<IFormFile> files,
            Guid categoryId,
            Guid? userId)
        {
            var attachments = new List<CategoryAttachment>();
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", _uploadsPath);

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            foreach (var file in files)
            {
                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                attachments.Add(new CategoryAttachment
                {
                    CategoryId = categoryId,
                    FileName = file.FileName,
                    FilePath = $"/{_uploadsPath}/{uniqueFileName}",
                    FileType = file.ContentType,
                    FileSize = file.Length,
                    UploadedBy = userId
                });
            }

            return attachments;
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
            catch (Exception)
            {
                // Log error but don't throw
            }
        }
    }
}
