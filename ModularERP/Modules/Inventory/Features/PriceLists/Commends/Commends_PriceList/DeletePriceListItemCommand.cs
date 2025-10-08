using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceList
{
    public class DeletePriceListItemCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid PriceListId { get; set; }
        public Guid ItemId { get; set; }
    }
}
