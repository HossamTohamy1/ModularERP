using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Purchases.Invoicing.Commends.Commend_InvocieItem
{
    public class DeleteInvoiceItemCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid InvoiceId { get; set; }
        public Guid ItemId { get; set; }

        public DeleteInvoiceItemCommand(Guid invoiceId, Guid itemId)
        {
            InvoiceId = invoiceId;
            ItemId = itemId;
        }
    }
}