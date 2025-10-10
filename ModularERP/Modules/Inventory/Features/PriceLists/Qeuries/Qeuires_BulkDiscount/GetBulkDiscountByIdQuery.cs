using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_BulkDiscount;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_BulkDiscount
{
    public class GetBulkDiscountByIdQuery : IRequest<ResponseViewModel<BulkDiscountDto>>
    {
        public Guid Id { get; set; }
        public Guid PriceListId { get; set; }
    }
}
