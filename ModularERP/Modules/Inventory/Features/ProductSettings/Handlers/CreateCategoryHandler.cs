using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers
{
    public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, ResponseViewModel<CategoryResponseDto>>
    {
        private readonly IGeneralRepository<Category> _categoryRepo;
        private readonly IGeneralRepository<CategoryAttachment> _attachmentRepo;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContext;
        private readonly string _uploadsPath = "uploads/categories";

        public CreateCategoryHandler(
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
            CreateCategoryCommand request,
            CancellationToken cancellationToken)
        {
            // Map DTO to Entity
            var category = _mapper.Map<Category>(request.Dto);

            // Add Category
            await _categoryRepo.AddAsync(category);
            await _categoryRepo.SaveChanges();

            // Handle Attachments
            if (request.Dto.Attachments != null && request.Dto.Attachments.Any())
            {
                var userId = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53");
                var attachments = await SaveAttachments(request.Dto.Attachments, category.Id, userId);

                await _attachmentRepo.AddRangeAsync(attachments);
                await _attachmentRepo.SaveChanges();
            }

            // Get created category with details
            var createdCategory = await _categoryRepo
                .Get(c => c.Id == category.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (createdCategory == null)
                throw new NotFoundException("Category not found after creation", FinanceErrorCode.NotFound);

            // Load related data manually (no Include)
            var parentCategory = createdCategory.ParentCategoryId.HasValue
                ? await _categoryRepo.GetByID(createdCategory.ParentCategoryId.Value)
                : null;

            var attachmentsList = await _attachmentRepo
                .Get(a => a.CategoryId == category.Id)
                .ToListAsync(cancellationToken);

            var subCategoriesCount = await _categoryRepo
                .Get(c => c.ParentCategoryId == category.Id)
                .CountAsync(cancellationToken);

            // Map to Response
            var response = _mapper.Map<CategoryResponseDto>(createdCategory);
            response.ParentCategoryName = parentCategory?.Name;
            response.Attachments = _mapper.Map<List<CategoryAttachmentDto>>(attachmentsList);
            response.SubCategoriesCount = subCategoriesCount;

            return ResponseViewModel<CategoryResponseDto>.Success(
                response,
                "Category created successfully");
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
    }
}
