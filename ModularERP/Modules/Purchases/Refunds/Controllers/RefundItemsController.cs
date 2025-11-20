using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundItem;
using ModularERP.Modules.Purchases.Refunds.Qeuries.Qeuries_RefundItem;

namespace ModularERP.Modules.Purchases.Refunds.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RefundItemsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RefundItemsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all items for a specific refund
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRefundItems(Guid refundId)
        {
            var query = new GetRefundItemsQuery { RefundId = refundId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get a specific refund item by ID
        /// </summary>
        [HttpGet("{itemId}")]
        public async Task<IActionResult> GetRefundItemById(Guid refundId, Guid itemId)
        {
            var query = new GetRefundItemByIdQuery
            {
                RefundId = refundId,
                ItemId = itemId
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Add a new item to a refund
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddRefundItem(
            Guid refundId,
            [FromBody] AddRefundItemRequest request)
        {
            var command = new AddRefundItemCommand
            {
                RefundId = refundId,
                GRNLineItemId = request.GRNLineItemId,
                ReturnQuantity = request.ReturnQuantity,
                UnitPrice = request.UnitPrice
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(
                nameof(GetRefundItemById),
                new { refundId, itemId = result.Data.Id },
                result);
        }

        /// <summary>
        /// Update an existing refund item
        /// </summary>
        [HttpPut("{itemId}")]
        public async Task<IActionResult> UpdateRefundItem(
            Guid refundId,
            Guid itemId,
            [FromBody] UpdateRefundItemRequest request)
        {
            var command = new UpdateRefundItemCommand
            {
                RefundId = refundId,
                ItemId = itemId,
                ReturnQuantity = request.ReturnQuantity,
                UnitPrice = request.UnitPrice
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Delete a refund item
        /// </summary>
        [HttpDelete("{itemId}")]
        public async Task<IActionResult> DeleteRefundItem(Guid refundId, Guid itemId)
        {
            var command = new DeleteRefundItemCommand
            {
                RefundId = refundId,
                ItemId = itemId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }

    // Request Models
    public record AddRefundItemRequest
    {
        public Guid GRNLineItemId { get; init; }
        public decimal ReturnQuantity { get; init; }
        public decimal UnitPrice { get; init; }
    }

    public record UpdateRefundItemRequest
    {
        public decimal ReturnQuantity { get; init; }
        public decimal UnitPrice { get; init; }
    }
}