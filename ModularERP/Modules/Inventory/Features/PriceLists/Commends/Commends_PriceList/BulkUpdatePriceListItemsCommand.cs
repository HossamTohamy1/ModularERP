using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceList;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceList
{
    public class BulkUpdatePriceListItemsCommand : IRequest<ResponseViewModel<List<PriceListItemDto>>>
    {
        public Guid PriceListId { get; set; }
        public BulkUpdatePriceListItemDto BulkItems { get; set; } = null!;
    }
}
