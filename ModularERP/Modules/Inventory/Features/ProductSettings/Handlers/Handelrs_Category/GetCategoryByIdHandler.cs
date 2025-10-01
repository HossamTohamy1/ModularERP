using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Category;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Qeuries_Category;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handelrs_Category
{
    public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, ResponseViewModel<CategoryResponseDto>>
    {
        private readonly IGeneralRepository<Category> _categoryRepo;
        private readonly IGeneralRepository<CategoryAttachment> _attachmentRepo;
        private readonly IMapper _mapper;

        public GetCategoryByIdHandler(
            IGeneralRepository<Category> categoryRepo,
            IGeneralRepository<CategoryAttachment> attachmentRepo,
            IMapper mapper)
        {
            _categoryRepo = categoryRepo;
            _attachmentRepo = attachmentRepo;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<CategoryResponseDto>> Handle(
            GetCategoryByIdQuery request,
            CancellationToken cancellationToken)
        {
            var category = await _categoryRepo.GetByID(request.Id);

            if (category == null)
                throw new NotFoundException("Category not found", FinanceErrorCode.NotFound);

            // Load parent category
            Category? parentCategory = null;
            if (category.ParentCategoryId.HasValue)
            {
                parentCategory = await _categoryRepo.GetByID(category.ParentCategoryId.Value);
            }

            // Load attachments
            var attachments = await _attachmentRepo
                .Get(a => a.CategoryId == request.Id)
                .ToListAsync(cancellationToken);

            // Count subcategories
            var subCategoriesCount = await _categoryRepo
                .Get(c => c.ParentCategoryId == request.Id)
                .CountAsync(cancellationToken);

            // Map to response
            var response = _mapper.Map<CategoryResponseDto>(category);
            response.ParentCategoryName = parentCategory?.Name;
            response.Attachments = _mapper.Map<List<CategoryAttachmentDto>>(attachments);
            response.SubCategoriesCount = subCategoriesCount;

            return ResponseViewModel<CategoryResponseDto>.Success(
                response,
                "Category retrieved successfully");
        }
    }
}
