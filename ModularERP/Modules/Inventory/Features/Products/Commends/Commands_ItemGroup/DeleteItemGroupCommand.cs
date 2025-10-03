using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.Products.Commends.Commands_ItemGroup
{
    public class DeleteItemGroupCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }
    }
}
