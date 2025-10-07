using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListItems
{
    public class ClonePriceListCommand : IRequest<ResponseViewModel<PriceListDto>>
    {
        public ClonePriceListDto CloneData { get; set; }

        public ClonePriceListCommand(ClonePriceListDto cloneData)
        {
            CloneData = cloneData;
        }
    }
}