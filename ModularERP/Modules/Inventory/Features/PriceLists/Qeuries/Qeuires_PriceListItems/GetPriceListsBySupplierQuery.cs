using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceListItems
{
    public class GetPriceListsBySupplierQuery : IRequest<ResponseViewModel<IEnumerable<PriceListListDto>>>
    {
        public Guid SupplierId { get; set; }

        public GetPriceListsBySupplierQuery(Guid supplierId)
        {
            SupplierId = supplierId;
        }
    }
}