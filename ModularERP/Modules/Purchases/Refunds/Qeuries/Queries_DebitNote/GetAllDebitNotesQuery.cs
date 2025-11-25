using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_DebitNote;

namespace ModularERP.Modules.Purchases.Refunds.Qeuries.Queries_DebitNote
{
    public class GetAllDebitNotesQuery : IRequest<ResponseViewModel<List<DebitNoteListDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? SupplierId { get; set; }
    }

}
