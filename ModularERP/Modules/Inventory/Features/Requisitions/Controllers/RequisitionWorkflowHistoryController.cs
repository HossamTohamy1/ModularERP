using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition_Workflow;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_ApprovalHistory;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition_Workflow;
using ModularERP.Modules.Inventory.Features.Requisitions.Qeuries.Qeuier_ApprovalHistory;
using ModularERP.Modules.Inventory.Features.Requisitions.Qeuries.Qeuries_Requisition_Workflow;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequisitionWorkflowHistoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RequisitionWorkflowHistoryController(IMediator mediator)
        {
            _mediator = mediator;
        }



        [HttpGet("{id}/approval-log")]
        public async Task<ActionResult<ResponseViewModel<List<ApprovalLogDto>>>> GetApprovalLog(
            Guid id,
            CancellationToken cancellationToken)
        {
            var query = new GetRequisitionApprovalLogQuery { RequisitionId = id };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id}/approval-history")]
        public async Task<ActionResult<ResponseViewModel<ApprovalHistoryDto>>> GetApprovalHistory(
            Guid id,
            CancellationToken cancellationToken)
        {
            var query = new GetRequisitionApprovalHistoryQuery { RequisitionId = id };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }


    }
}