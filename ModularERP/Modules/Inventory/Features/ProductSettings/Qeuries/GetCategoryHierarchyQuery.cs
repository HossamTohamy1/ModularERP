using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries
{
    public record GetCategoryHierarchyQuery
        : IRequest<ResponseViewModel<List<CategoryHierarchyDto>>>;
}
