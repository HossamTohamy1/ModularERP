using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_Invoice;

namespace ModularERP.Modules.Purchases.Invoicing.Qeuries.Qeuires_Invoice
{
    public class GetPurchaseInvoiceByIdQuery : IRequest<ResponseViewModel<PurchaseInvoiceDto>>
    {
        public Guid Id { get; set; }
    }
}
