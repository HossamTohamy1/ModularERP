using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_ApprovalHistory;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Qeuries.Qeuier_ApprovalHistory
{
    public class GetRequisitionApprovalHistoryQuery : IRequest<ResponseViewModel<ApprovalHistoryDto>>
    {
        public Guid RequisitionId { get; set; }
    }
}