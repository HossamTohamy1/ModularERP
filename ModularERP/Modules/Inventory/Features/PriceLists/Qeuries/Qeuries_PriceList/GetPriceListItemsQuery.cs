using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceList;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuries_PriceList
{
    public class GetPriceListItemsQuery : IRequest<ResponseViewModel<List<PriceListItemListDto>>>
    {
        public Guid PriceListId { get; set; }
        public bool IncludeInactive { get; set; } = false;
        public DateTime? AsOfDate { get; set; }
    }
}
