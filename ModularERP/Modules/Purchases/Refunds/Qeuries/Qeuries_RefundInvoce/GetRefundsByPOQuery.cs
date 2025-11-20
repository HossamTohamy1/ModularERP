using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;

namespace ModularERP.Modules.Purchases.Refunds.Qeuries.Qeuries_RefundInvoce
{
    public class GetRefundsByPOQuery : IRequest<ResponseViewModel<List<RefundDto>>>
    {
        public Guid PurchaseOrderId { get; set; }
    }
}
