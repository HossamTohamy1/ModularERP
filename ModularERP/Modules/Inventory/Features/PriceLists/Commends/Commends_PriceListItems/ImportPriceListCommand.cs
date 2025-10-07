using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListItems
{
    public class ImportPriceListCommand : IRequest<ResponseViewModel<bool>>
    {
        public ImportPriceListDto ImportData { get; set; }

        public ImportPriceListCommand(ImportPriceListDto importData)
        {
            ImportData = importData;
        }
    }
}
