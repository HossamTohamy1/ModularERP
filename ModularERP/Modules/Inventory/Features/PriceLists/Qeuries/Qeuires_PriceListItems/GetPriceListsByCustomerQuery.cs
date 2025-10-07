using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceListItems
{
    public class GetPriceListsByCustomerQuery : IRequest<ResponseViewModel<IEnumerable<PriceListListDto>>>
    {
        public Guid CustomerId { get; set; }

        public GetPriceListsByCustomerQuery(Guid customerId)
        {
            CustomerId = customerId;
        }
    }
}
