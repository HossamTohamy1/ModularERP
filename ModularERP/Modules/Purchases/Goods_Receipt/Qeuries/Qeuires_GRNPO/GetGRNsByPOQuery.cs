using MediatR;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Qeuries.Qeuires_GRNPO
{
    public class GetGRNsByPOQuery : IRequest<List<GRNListItemDto>>
    {
        public Guid PurchaseOrderId { get; set; }
        public Guid CompanyId { get; set; }
    }
}