using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;

namespace ModularERP.Modules.Purchases.Refunds.Qeuries.Qeuries_RefundInvoce
{
    public class GetRefundsByInvoiceQuery : IRequest<ResponseViewModel<List<RefundDto>>>
    {
        public Guid InvoiceId { get; set; }
    }
}
