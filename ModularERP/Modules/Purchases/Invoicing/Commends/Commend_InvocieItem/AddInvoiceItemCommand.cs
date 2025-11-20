using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem;

namespace ModularERP.Modules.Purchases.Invoicing.Commends.Commend_InvocieItem
{
    public class AddInvoiceItemCommand : IRequest<ResponseViewModel<InvoiceLineItemResponse>>
    {
        public Guid InvoiceId { get; set; }
        public AddInvoiceItemRequest Request { get; set; }

        public AddInvoiceItemCommand(Guid invoiceId, AddInvoiceItemRequest request)
        {
            InvoiceId = invoiceId;
            Request = request;
        }
    }
}