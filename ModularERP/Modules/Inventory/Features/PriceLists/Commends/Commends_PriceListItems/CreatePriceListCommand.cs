using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListItems
{
    public class CreatePriceListCommand : IRequest<ResponseViewModel<PriceListDto>>
    {
        public CreatePriceListDto PriceList { get; set; }

        public CreatePriceListCommand(CreatePriceListDto priceList)
        {
            PriceList = priceList;
        }
    }
}
