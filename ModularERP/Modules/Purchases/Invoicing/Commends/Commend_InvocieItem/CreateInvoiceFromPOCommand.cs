using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem;

namespace ModularERP.Modules.Purchases.Invoicing.Commends.Commend_InvocieItem
{
    public class CreateInvoiceFromPOCommand : IRequest<ResponseViewModel<InvoiceResponse>>
    {
        public Guid PurchaseOrderId { get; set; }
        public CreateInvoiceFromPORequest Request { get; set; }

        public CreateInvoiceFromPOCommand(Guid purchaseOrderId, CreateInvoiceFromPORequest request)
        {
            PurchaseOrderId = purchaseOrderId;
            Request = request;
        }
    }
}

