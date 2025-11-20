using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Purchases.Invoicing.Commends.Commands_Invoice
{
    public class DeletePurchaseInvoiceCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }
    }
}