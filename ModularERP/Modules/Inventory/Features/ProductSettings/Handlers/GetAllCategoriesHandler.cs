using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers
{
    public class GetAllCategoriesHandler : IRequestHandler<GetAllCategoriesQuery, ResponseViewModel<PaginatedResult<CategoryListDto>>>
    {
        private readonly IGeneralRepository<Category> _categoryRepo;
        private readonly IGeneralRepository<CategoryAttachment> _attachmentRepo;
        private readonly IMapper _mapper;

        public GetAllCategoriesHandler(
            IGeneralRepository<Category> categoryRepo,
            IGeneralRepository<CategoryAttachment> attachmentRepo,
            IMapper mapper)
        {
            _categoryRepo = categoryRepo;
            _attachmentRepo = attachmentRepo;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<PaginatedResult<CategoryListDto>>> Handle(
            GetAllCategoriesQuery request,
            CancellationToken cancellationToken)
        {
            var query = _categoryRepo.GetAll();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(searchTerm) ||
                    (c.Description != null && c.Description.ToLower().Contains(searchTerm)));
            }

            // Filter by parent category
            if (request.ParentCategoryId.HasValue)
            {
                query = query.Where(c => c.ParentCategoryId == request.ParentCategoryId.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var categories = await query
                .OrderBy(c => c.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var categoryIds = categories.Select(c => c.Id).ToList();

            // ✅ جلب المرفقات مرة واحدة لكل الكاتيجوري
            var attachmentsLookup = await _attachmentRepo
                .Get(a => categoryIds.Contains(a.CategoryId))
                .GroupBy(a => a.CategoryId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.ToList(),
                    cancellationToken);

            // Build DTOs
            var categoryDtos = new List<CategoryListDto>();

            foreach (var category in categories)
            {
                var dto = _mapper.Map<CategoryListDto>(category);

                // Get parent category name if exists
                if (category.ParentCategoryId.HasValue)
                {
                    var parent = await _categoryRepo.GetByID(category.ParentCategoryId.Value);
                    dto.ParentCategoryName = parent?.Name;
                }

                // Count subcategories
                dto.SubCategoriesCount = await _categoryRepo
                    .Get(c => c.ParentCategoryId == category.Id)
                    .CountAsync(cancellationToken);

                // ✅ Attachments mapped
                if (attachmentsLookup.ContainsKey(category.Id))
                {
                    dto.Attachments = _mapper.Map<List<CategoryAttachmentDto>>(attachmentsLookup[category.Id]);
                    dto.AttachmentsCount = dto.Attachments.Count;
                }
                else
                {
                    dto.Attachments = new List<CategoryAttachmentDto>();
                    dto.AttachmentsCount = 0;
                }

                categoryDtos.Add(dto);
            }

            var result = new PaginatedResult<CategoryListDto>
            {
                Items = categoryDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return ResponseViewModel<PaginatedResult<CategoryListDto>>.Success(
                result,
                "Categories retrieved successfully");
        }
    }
}
