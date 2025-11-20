using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem;

namespace ModularERP.Modules.Purchases.Invoicing.Qeuries.Qeuires_InvociePayment
{
    public class GetInvoicePaymentsQuery : IRequest<ResponseViewModel<GetInvoicePaymentsResponse>>
    {
        public Guid InvoiceId { get; set; }
    }
}
