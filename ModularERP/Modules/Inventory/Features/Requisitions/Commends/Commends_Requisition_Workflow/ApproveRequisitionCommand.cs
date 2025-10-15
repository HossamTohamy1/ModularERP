using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition_Workflow
{
    public class ApproveRequisitionCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid RequisitionId { get; set; }
        public string? Comments { get; set; }
    }
}