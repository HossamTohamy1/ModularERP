using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Commends
{
    public record UpdateCategoryCommand(UpdateCategoryDto Dto)
        : IRequest<ResponseViewModel<CategoryResponseDto>>;
}
