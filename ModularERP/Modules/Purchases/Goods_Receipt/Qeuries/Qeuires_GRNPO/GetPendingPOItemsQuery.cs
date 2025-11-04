using MediatR;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Qeuries.Qeuires_GRNPO
{
    public class GetPendingPOItemsQuery : IRequest<List<PendingPOItemDto>>
    {
        public Guid PurchaseOrderId { get; set; }
        public Guid CompanyId { get; set; }
    }
}