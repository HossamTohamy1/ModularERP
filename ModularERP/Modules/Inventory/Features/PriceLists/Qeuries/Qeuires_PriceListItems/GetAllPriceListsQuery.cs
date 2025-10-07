using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceListItems
{
    public class GetAllPriceListsQuery : IRequest<ResponseViewModel<IEnumerable<PriceListListDto>>>
    {
        public PriceListFilterDto Filter { get; set; }

        public GetAllPriceListsQuery(PriceListFilterDto filter)
        {
            Filter = filter;
        }
    }
}
