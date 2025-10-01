using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers
{
    public class GetCategoryHierarchyHandler : IRequestHandler<GetCategoryHierarchyQuery, ResponseViewModel<List<CategoryHierarchyDto>>>
    {
        private readonly IGeneralRepository<Category> _categoryRepo;

        public GetCategoryHierarchyHandler(IGeneralRepository<Category> categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<ResponseViewModel<List<CategoryHierarchyDto>>> Handle(
            GetCategoryHierarchyQuery request,
            CancellationToken cancellationToken)
        {
            var allCategories = await _categoryRepo
                .GetAll()
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);

            var hierarchy = BuildHierarchy(allCategories, null, 0);

            return ResponseViewModel<List<CategoryHierarchyDto>>.Success(
                hierarchy,
                "Category hierarchy retrieved successfully");
        }

        private List<CategoryHierarchyDto> BuildHierarchy(
            List<Category> allCategories,
            Guid? parentId,
            int level)
        {
            var result = new List<CategoryHierarchyDto>();
            var children = allCategories.Where(c => c.ParentCategoryId == parentId).ToList();

            foreach (var category in children)
            {
                var indent = new string('-', level * 2);
                var dto = new CategoryHierarchyDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    ParentCategoryId = category.ParentCategoryId,
                    Level = level,
                    DisplayName = level > 0 ? $"{indent} {category.Name}" : category.Name
                };

                result.Add(dto);

                // Recursively add children
                result.AddRange(BuildHierarchy(allCategories, category.Id, level + 1));
            }

            return result;
        }
    }
}
