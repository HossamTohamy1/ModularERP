using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_DebitNote;

namespace ModularERP.Modules.Purchases.Refunds.Qeuries.Queries_DebitNote
{
    public class GetDebitNoteRefundQuery : IRequest<ResponseViewModel<RefundSummaryDto>>
    {
        public Guid DebitNoteId { get; set; }

        public GetDebitNoteRefundQuery(Guid debitNoteId)
        {
            DebitNoteId = debitNoteId;
        }
    }
}
