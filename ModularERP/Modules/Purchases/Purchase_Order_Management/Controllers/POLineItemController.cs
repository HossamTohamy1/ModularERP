using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_POItme;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POItme;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_POItme;
using Serilog;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Controllers
{
    [Route("api/purchase-orders/{purchaseOrderId}/items")]
    [ApiController]
    public class POLineItemController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<POLineItemController> _logger;

        public POLineItemController(IMediator mediator, ILogger<POLineItemController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all line items for a purchase order
        /// </summary>
        /// <param name="purchaseOrderId">Purchase Order ID</param>
        /// <returns>List of line items</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseViewModel<List<POLineItemListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLineItems([FromRoute] Guid purchaseOrderId)
        {
            _logger.LogInformation("GET request received for line items of PO {PurchaseOrderId}", purchaseOrderId);

            var query = new GetPOLineItemsQuery { PurchaseOrderId = purchaseOrderId };
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Get a specific line item by ID
        /// </summary>
        /// <param name="purchaseOrderId">Purchase Order ID</param>
        /// <param name="itemId">Line Item ID</param>
        /// <returns>Line item details</returns>
        [HttpGet("{itemId}")]
        [ProducesResponseType(typeof(ResponseViewModel<POLineItemResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLineItemById(
            [FromRoute] Guid purchaseOrderId,
            [FromRoute] Guid itemId)
        {
            _logger.LogInformation("GET request received for line item {ItemId} of PO {PurchaseOrderId}",
                itemId, purchaseOrderId);

            var query = new GetPOLineItemByIdQuery
            {
                PurchaseOrderId = purchaseOrderId,
                LineItemId = itemId
            };
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Add a new line item to a purchase order
        /// </summary>
        /// <param name="purchaseOrderId">Purchase Order ID</param>
        /// <param name="lineItem">Line item data</param>
        /// <returns>Created line item</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseViewModel<POLineItemResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateLineItem(
            [FromRoute] Guid purchaseOrderId,
            [FromBody] CreatePOLineItemDto lineItem)
        {
            _logger.LogInformation("POST request received to create line item for PO {PurchaseOrderId}",
                purchaseOrderId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for creating line item");
                return BadRequest(ResponseViewModel<bool>.ValidationError(
                    "Validation failed",
                    ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    )
                ));
            }

            var command = new CreatePOLineItemCommand
            {
                PurchaseOrderId = purchaseOrderId,
                LineItem = lineItem
            };

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return CreatedAtAction(
                    nameof(GetLineItemById),
                    new { purchaseOrderId, itemId = result.Data!.Id },
                    result
                );
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Update an existing line item
        /// </summary>
        /// <param name="purchaseOrderId">Purchase Order ID</param>
        /// <param name="itemId">Line Item ID</param>
        /// <param name="lineItem">Updated line item data</param>
        /// <returns>Updated line item</returns>
        [HttpPut("{itemId}")]
        [ProducesResponseType(typeof(ResponseViewModel<POLineItemResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateLineItem(
            [FromRoute] Guid purchaseOrderId,
            [FromRoute] Guid itemId,
            [FromBody] UpdatePOLineItemDto lineItem)
        {
            _logger.LogInformation("PUT request received to update line item {ItemId} of PO {PurchaseOrderId}",
                itemId, purchaseOrderId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for updating line item");
                return BadRequest(ResponseViewModel<bool>.ValidationError(
                    "Validation failed",
                    ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    )
                ));
            }

            // Ensure IDs match
            if (lineItem.Id != itemId)
            {
                return BadRequest(ResponseViewModel<bool>.Error(
                    "Line item ID in URL does not match ID in request body",
                    Common.Enum.Finance_Enum.FinanceErrorCode.ValidationError
                ));
            }

            if (lineItem.PurchaseOrderId != purchaseOrderId)
            {
                return BadRequest(ResponseViewModel<bool>.Error(
                    "Purchase Order ID in URL does not match ID in request body",
                    Common.Enum.Finance_Enum.FinanceErrorCode.ValidationError
                ));
            }

            var command = new UpdatePOLineItemCommand
            {
                PurchaseOrderId = purchaseOrderId,
                LineItemId = itemId,
                LineItem = lineItem
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Delete a line item from a purchase order
        /// </summary>
        /// <param name="purchaseOrderId">Purchase Order ID</param>
        /// <param name="itemId">Line Item ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{itemId}")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteLineItem(
            [FromRoute] Guid purchaseOrderId,
            [FromRoute] Guid itemId)
        {
            _logger.LogInformation("DELETE request received for line item {ItemId} of PO {PurchaseOrderId}",
                itemId, purchaseOrderId);

            var command = new DeletePOLineItemCommand
            {
                PurchaseOrderId = purchaseOrderId,
                LineItemId = itemId
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}
