using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_DebitNote;

namespace ModularERP.Modules.Purchases.Refunds.Qeuries.Queries_DebitNote
{
    public class GetDebitNoteByIdQuery : IRequest<ResponseViewModel<DebitNoteDetailsDto>>
    {
        public Guid Id { get; set; }

        public GetDebitNoteByIdQuery(Guid id)
        {
            Id = id;
        }
    }

}
