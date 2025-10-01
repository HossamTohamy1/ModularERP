using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Category;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Qeuries_Category
{
    public record GetCategoryByIdQuery(Guid Id)
        : IRequest<ResponseViewModel<CategoryResponseDto>>;
}
