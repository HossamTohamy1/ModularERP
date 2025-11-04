using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Qeuries.Qeuries_GRN
{
    public class GetAllGRNsQuery : IRequest<ResponseViewModel<List<GRNListItemDto>>>
    {
        public Guid CompanyId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? WarehouseId { get; set; }
        public Guid? PurchaseOrderId { get; set; }

        public GetAllGRNsQuery(Guid companyId, DateTime? fromDate = null, DateTime? toDate = null,
            Guid? warehouseId = null, Guid? purchaseOrderId = null)
        {
            CompanyId = companyId;
            FromDate = fromDate;
            ToDate = toDate;
            WarehouseId = warehouseId;
            PurchaseOrderId = purchaseOrderId;
        }
    }
}
