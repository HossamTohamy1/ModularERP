using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_PurchaseOrder;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PurchaseOrderController> _logger;

        public PurchaseOrderController(IMediator mediator, ILogger<PurchaseOrderController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Create a new purchase order
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseViewModel<CreatePurchaseOrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePurchaseOrder([FromForm] CreatePurchaseOrderCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Get purchase order list with filters and pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseViewModel<PagedResult<PurchaseOrderListDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPurchaseOrders([FromQuery] GetPurchaseOrderListQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get purchase order details by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<PurchaseOrderDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPurchaseOrder(Guid id)
        {
            var query = new GetPurchaseOrderQuery { Id = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Update purchase order (Draft only)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePurchaseOrder(Guid id, [FromForm] UpdatePurchaseOrderCommand command)
        {
            if (id != command.Id)
                return BadRequest(ResponseViewModel<bool>.Error(
                    "ID mismatch",
                    Common.Enum.Finance_Enum.FinanceErrorCode.ValidationError));

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Delete purchase order (Draft only)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePurchaseOrder(Guid id)
        {
            var command = new DeletePurchaseOrderCommand { Id = id };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Submit purchase order for approval
        /// </summary>
        [HttpPost("{id}/submit")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SubmitPurchaseOrder(Guid id, [FromBody] SubmitPurchaseOrderRequest request)
        {
            var command = new SubmitPurchaseOrderCommand
            {
                Id = id,
                SubmittedBy = request.SubmittedBy
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Approve purchase order
        /// </summary>
        [HttpPost("{id}/approve")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApprovePurchaseOrder(Guid id, [FromBody] ApprovePurchaseOrderRequest request)
        {
            var command = new ApprovePurchaseOrderCommand
            {
                Id = id,
                ApprovedBy = request.ApprovedBy,
                ApprovalNotes = request.ApprovalNotes
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Reject purchase order (return to draft)
        /// </summary>
        [HttpPost("{id}/reject")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RejectPurchaseOrder(Guid id, [FromBody] RejectPurchaseOrderRequest request)
        {
            var command = new RejectPurchaseOrderCommand
            {
                Id = id,
                RejectedBy = request.RejectedBy,
                RejectionReason = request.RejectionReason
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Cancel purchase order
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelPurchaseOrder(Guid id, [FromBody] CancelPurchaseOrderRequest request)
        {
            var command = new CancelPurchaseOrderCommand
            {
                Id = id,
                CancellationReason = request.CancellationReason
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Close purchase order (when fully received and paid)
        /// </summary>
        [HttpPost("{id}/close")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ClosePurchaseOrder(Guid id, [FromBody] ClosePurchaseOrderRequest request)
        {
            var command = new ClosePurchaseOrderCommand
            {
                Id = id,
                ClosedBy = request.ClosedBy
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        #region Request Models
        public class SubmitPurchaseOrderRequest
        {
            public Guid SubmittedBy { get; set; }
        }

        public class ApprovePurchaseOrderRequest
        {
            public Guid ApprovedBy { get; set; }
            public string? ApprovalNotes { get; set; }
        }

        public class RejectPurchaseOrderRequest
        {
            public Guid RejectedBy { get; set; }
            public string RejectionReason { get; set; } = string.Empty;
        }

        public class CancelPurchaseOrderRequest
        {
            public string CancellationReason { get; set; } = string.Empty;
        }

        public class ClosePurchaseOrderRequest
        {
            public Guid ClosedBy { get; set; }
        }
        #endregion
    }
}