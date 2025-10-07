using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceListItems
{
    public class GetPriceListByIdQuery : IRequest<ResponseViewModel<PriceListDto>>
    {
        public Guid Id { get; set; }

        public GetPriceListByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
