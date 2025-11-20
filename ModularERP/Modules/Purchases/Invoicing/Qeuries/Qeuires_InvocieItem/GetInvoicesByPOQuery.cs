using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem;

namespace ModularERP.Modules.Purchases.Invoicing.Qeuries.Qeuires_InvocieItem
{
    public class GetInvoicesByPOQuery : IRequest<ResponseViewModel<List<InvoiceResponse>>>
    {
        public Guid PurchaseOrderId { get; set; }

        public GetInvoicesByPOQuery(Guid purchaseOrderId)
        {
            PurchaseOrderId = purchaseOrderId;
        }
    }
}
