using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_RequisitionItem;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;
using ModularERP.Modules.Inventory.Features.Requisitions.Qeuries.Qeuier_RequisitionItem;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Controllers
{
    [ApiController]
    [Route("api/requisitions/{requisitionId}/items")]
    public class RequisitionItemsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public RequisitionItemsController(IMediator mediator)
        {
            _mediator = mediator;
            _logger = Log.ForContext<RequisitionItemsController>();
        }

        /// <summary>
        /// Get all items for a specific requisition
        /// </summary>
        /// <param name="requisitionId">Requisition ID</param>
        /// <returns>List of requisition items</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseViewModel<List<RequisitionItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRequisitionItems([FromRoute] Guid requisitionId)
        {
            _logger.Information("GET request received for requisition {RequisitionId} items", requisitionId);

            var query = new GetRequisitionItemsQuery { RequisitionId = requisitionId };
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Get a specific item by ID
        /// </summary>
        /// <param name="requisitionId">Requisition ID</param>
        /// <param name="itemId">Item ID</param>
        /// <returns>Requisition item details</returns>
        [HttpGet("{itemId}")]
        [ProducesResponseType(typeof(ResponseViewModel<RequisitionItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRequisitionItemById(
            [FromRoute] Guid requisitionId,
            [FromRoute] Guid itemId)
        {
            _logger.Information("GET request received for item {ItemId} in requisition {RequisitionId}",
                itemId, requisitionId);

            var query = new GetRequisitionItemByIdQuery
            {
                RequisitionId = requisitionId,
                ItemId = itemId
            };
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Create a new item for a requisition
        /// </summary>
        /// <param name="requisitionId">Requisition ID</param>
        /// <param name="dto">Item data</param>
        /// <returns>Created item</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseViewModel<RequisitionItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateRequisitionItem(
            [FromRoute] Guid requisitionId,
            [FromBody] CreateRequisitionItemDto dto)
        {
            _logger.Information("POST request received to create item for requisition {RequisitionId}", requisitionId);

            var command = new CreateRequisitionItemCommand
            {
                RequisitionId = requisitionId,
                Item = dto
            };
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Update an existing item
        /// </summary>
        /// <param name="requisitionId">Requisition ID</param>
        /// <param name="itemId">Item ID</param>
        /// <param name="dto">Updated item data</param>
        /// <returns>Updated item</returns>
        [HttpPut("{itemId}")]
        [ProducesResponseType(typeof(ResponseViewModel<RequisitionItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRequisitionItem(
            [FromRoute] Guid requisitionId,
            [FromRoute] Guid itemId,
            [FromBody] UpdateRequisitionItemDTO dto)
        {
            _logger.Information("PUT request received to update item {ItemId} in requisition {RequisitionId}",
                itemId, requisitionId);

            var command = new UpdateRequisitionItemCommand
            {
                RequisitionId = requisitionId,
                ItemId = itemId,
                Item = dto
            };
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Delete an item
        /// </summary>
        /// <param name="requisitionId">Requisition ID</param>
        /// <param name="itemId">Item ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{itemId}")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRequisitionItem(
            [FromRoute] Guid requisitionId,
            [FromRoute] Guid itemId)
        {
            _logger.Information("DELETE request received for item {ItemId} in requisition {RequisitionId}",
                itemId, requisitionId);

            var command = new DeleteRequisitionItemCommand
            {
                RequisitionId = requisitionId,
                ItemId = itemId
            };
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Bulk create multiple items at once
        /// </summary>
        /// <param name="requisitionId">Requisition ID</param>
        /// <param name="dto">List of items to create</param>
        /// <returns>Created items</returns>
        [HttpPost("bulk")]
        [ProducesResponseType(typeof(ResponseViewModel<List<RequisitionItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BulkCreateRequisitionItems(
            [FromRoute] Guid requisitionId,
            [FromBody] BulkCreateRequisitionItemDTO dto)
        {
            _logger.Information("POST request received to bulk create {Count} items for requisition {RequisitionId}",
                dto.Items.Count, requisitionId);

            var command = new BulkCreateRequisitionItemsCommand
            {
                RequisitionId = requisitionId,
                BulkItems = dto
            };
            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}