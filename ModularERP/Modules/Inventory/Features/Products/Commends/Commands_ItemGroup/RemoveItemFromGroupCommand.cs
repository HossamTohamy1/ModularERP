using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.Products.Commends.Commands_ItemGroup
{
    public class RemoveItemFromGroupCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid GroupId { get; set; }
        public Guid ItemId { get; set; }
    }
}
