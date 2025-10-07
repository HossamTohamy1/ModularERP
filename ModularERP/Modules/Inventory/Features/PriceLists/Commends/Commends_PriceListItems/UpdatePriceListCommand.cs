using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListItems
{
    public class UpdatePriceListCommand : IRequest<ResponseViewModel<PriceListDto>>
    {
        public UpdatePriceListDto PriceList { get; set; }

        public UpdatePriceListCommand(UpdatePriceListDto priceList)
        {
            PriceList = priceList;
        }
    }
}