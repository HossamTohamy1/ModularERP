using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries
{
    public record GetCategoryByIdQuery(Guid Id)
        : IRequest<ResponseViewModel<CategoryResponseDto>>;
}
