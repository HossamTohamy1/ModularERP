using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition_Workflow;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Qeuries.Qeuries_Requisition_Workflow
{
    public class GetWorkflowStatusQuery : IRequest<ResponseViewModel<WorkflowStatusDto>>
    {
        public Guid RequisitionId { get; set; }
    }

}
