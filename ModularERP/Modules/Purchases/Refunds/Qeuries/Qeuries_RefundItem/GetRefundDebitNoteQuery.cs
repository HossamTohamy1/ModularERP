using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;

namespace ModularERP.Modules.Purchases.Refunds.Queries.Queries_RefundItem
{
    public record GetRefundDebitNoteQuery : IRequest<ResponseViewModel<DebitNoteDto>>
    {
        public Guid RefundId { get; init; }
    }
}