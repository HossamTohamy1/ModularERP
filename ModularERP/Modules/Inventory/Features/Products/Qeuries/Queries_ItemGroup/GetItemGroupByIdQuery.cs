using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ItemGroup;

namespace ModularERP.Modules.Inventory.Features.Products.Qeuries.Queries_ItemGroup
{
    public class GetItemGroupByIdQuery : IRequest<ResponseViewModel<ItemGroupDetailDto>>
    {
        public Guid Id { get; set; }
    }
}
