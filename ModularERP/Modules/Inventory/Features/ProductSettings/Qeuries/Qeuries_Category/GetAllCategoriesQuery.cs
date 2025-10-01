using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Category;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Qeuries_Category
{
    public record GetAllCategoriesQuery(
         string? SearchTerm = null,
         Guid? ParentCategoryId = null,
         int PageNumber = 1,
         int PageSize = 10
     ) : IRequest<ResponseViewModel<PaginatedResult<CategoryListDto>>>;
}
