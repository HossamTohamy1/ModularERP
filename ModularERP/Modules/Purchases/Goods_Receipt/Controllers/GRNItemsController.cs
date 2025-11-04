using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRNItem;
using System.Security.Claims;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Controllers
{
    [Route("api/goods-receipts/{id}/items")]
    [ApiController]
    public class GRNItemsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GRNItemsController> _logger;

        public GRNItemsController(IMediator mediator, ILogger<GRNItemsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Add line item to existing GRN
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddGRNItem(
            [FromRoute] Guid id,
            [FromBody] AddGRNItemCommand command)
        {
            _logger.LogInformation("Adding item to GRN {GRNId}", id);

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            command.GRNId = id;
            command.UserId = userId;

            var result = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(AddGRNItem),
                new { id = result.Id },
                new
                {
                    success = true,
                    data = result,
                    message = "GRN item added successfully"
                });
        }

        /// <summary>
        /// Update GRN line item
        /// </summary>
        [HttpPut("{itemId}")]
        public async Task<IActionResult> UpdateGRNItem(
            [FromRoute] Guid id,
            [FromRoute] Guid itemId,
            [FromBody] UpdateGRNItemCommand command)
        {
            _logger.LogInformation("Updating item {ItemId} in GRN {GRNId}", itemId, id);

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            command.GRNId = id;
            command.ItemId = itemId;
            command.UserId = userId;

            var result = await _mediator.Send(command);

            return Ok(new
            {
                success = true,
                data = result,
                message = "GRN item updated successfully"
            });
        }

        /// <summary>
        /// Delete GRN line item
        /// </summary>
        [HttpDelete("{itemId}")]
        public async Task<IActionResult> DeleteGRNItem(
            [FromRoute] Guid id,
            [FromRoute] Guid itemId)
        {
            _logger.LogInformation("Deleting item {ItemId} from GRN {GRNId}", itemId, id);

            var command = new DeleteGRNItemCommand
            {
                GRNId = id,
                ItemId = itemId
            };

            await _mediator.Send(command);

            return Ok(new
            {
                success = true,
                message = "GRN item deleted successfully"
            });
        }
    }
}