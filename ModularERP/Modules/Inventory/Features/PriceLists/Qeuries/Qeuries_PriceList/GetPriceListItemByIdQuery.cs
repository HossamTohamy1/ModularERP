using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceList;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuries_PriceList
{
    public class GetPriceListItemByIdQuery : IRequest<ResponseViewModel<PriceListItemDto>>
    {
        public Guid PriceListId { get; set; }
        public Guid ItemId { get; set; }
    }
}
