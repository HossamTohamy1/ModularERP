using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_PurchaseOrder
{
    public class GetPurchaseOrderQuery : IRequest<ResponseViewModel<PurchaseOrderDetailDto>>
    {
        public Guid Id { get; set; }
    }

}
