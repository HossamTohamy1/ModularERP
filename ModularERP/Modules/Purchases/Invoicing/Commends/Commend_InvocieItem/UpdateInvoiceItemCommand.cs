using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem;

namespace ModularERP.Modules.Purchases.Invoicing.Commends.Commend_InvocieItem
{
    public class UpdateInvoiceItemCommand : IRequest<ResponseViewModel<InvoiceLineItemResponse>>
    {
        public Guid InvoiceId { get; set; }
        public Guid ItemId { get; set; }
        public UpdateInvoiceItemRequest Request { get; set; }

        public UpdateInvoiceItemCommand(Guid invoiceId, Guid itemId, UpdateInvoiceItemRequest request)
        {
            InvoiceId = invoiceId;
            ItemId = itemId;
            Request = request;
        }
    }
}