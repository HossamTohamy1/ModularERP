using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Category;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Category
{
    public record UpdateCategoryCommand(UpdateCategoryDto Dto)
        : IRequest<ResponseViewModel<CategoryResponseDto>>;
}
