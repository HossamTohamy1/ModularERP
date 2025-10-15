using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition_Workflow;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition_Workflow;
using ModularERP.Modules.Inventory.Features.Requisitions.Qeuries.Qeuries_Requisition_Workflow;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequisitionWorkflowController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RequisitionWorkflowController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Submit a draft requisition for approval
        /// </summary>
        [HttpPost("{id}/submit")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubmitRequisition(Guid id, [FromBody] SubmitRequisitionCommand command)
        {
            command.RequisitionId = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Approve a submitted requisition
        /// </summary>
        [HttpPost("{id}/approve")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveRequisition(Guid id, [FromBody] ApproveRequisitionCommand command)
        {
            command.RequisitionId = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Reject a submitted requisition
        /// </summary>
        [HttpPost("{id}/reject")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RejectRequisition(Guid id, [FromBody] RejectRequisitionCommand command)
        {
            command.RequisitionId = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Confirm an approved requisition and update stock
        /// </summary>
        [HttpPost("{id}/confirm")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmRequisition(Guid id, [FromBody] ConfirmRequisitionCommand command)
        {
            command.RequisitionId = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Cancel a requisition before confirmation
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelRequisition(Guid id, [FromBody] CancelRequisitionCommand command)
        {
            command.RequisitionId = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Reverse a confirmed requisition by creating an opposite transaction
        /// </summary>
        [HttpPost("{id}/reverse")]
        [ProducesResponseType(typeof(ResponseViewModel<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<Guid>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<Guid>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReverseRequisition(Guid id, [FromBody] ReverseRequisitionCommand command)
        {
            command.RequisitionId = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Get the complete workflow status and approval history of a requisition
        /// </summary>
        [HttpGet("{id}/workflow-status")]
        [ProducesResponseType(typeof(ResponseViewModel<WorkflowStatusDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<WorkflowStatusDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkflowStatus(Guid id)
        {
            var query = new GetWorkflowStatusQuery { RequisitionId = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}