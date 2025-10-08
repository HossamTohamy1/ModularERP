using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceList;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceList
{
    public class UpdatePriceListItemCommand : IRequest<ResponseViewModel<PriceListItemDto>>
    {
        public Guid PriceListId { get; set; }
        public Guid ItemId { get; set; }
        public UpdatePriceListItemDto Item { get; set; } = null!;
    }
}
