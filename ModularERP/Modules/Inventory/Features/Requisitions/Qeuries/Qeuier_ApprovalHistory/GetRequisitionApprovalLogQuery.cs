using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition_Workflow;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Qeuries.Qeuier_ApprovalHistory
{
    public class GetRequisitionApprovalLogQuery : IRequest<ResponseViewModel<List<ApprovalLogDto>>>
    {
        public Guid RequisitionId { get; set; }
    }
}