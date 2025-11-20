using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commend_InvocieItem;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem;
using ModularERP.Modules.Purchases.Invoicing.Qeuries.Qeuires_InvocieItem;

namespace ModularERP.Modules.Purchases.Invoicing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseInvoiceItemController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PurchaseInvoiceController> _logger;

        public PurchaseInvoiceItemController(IMediator mediator, ILogger<PurchaseInvoiceController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all invoices for a specific purchase order
        /// </summary>
        [HttpGet("purchase-orders/{poId}/invoices")]
        [ProducesResponseType(typeof(ResponseViewModel<List<InvoiceResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInvoicesByPO(Guid poId)
        {
            _logger.LogInformation("Received request to get invoices for PO: {PoId}", poId);

            var query = new GetInvoicesByPOQuery(poId);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Create invoice from purchase order
        /// </summary>
        [HttpPost("purchase-orders/{poId}/create-invoice")]
        [ProducesResponseType(typeof(ResponseViewModel<InvoiceResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateInvoiceFromPO(Guid poId, [FromBody] CreateInvoiceFromPORequest request)
        {
            _logger.LogInformation("Received request to create invoice from PO: {PoId}", poId);

            var command = new CreateInvoiceFromPOCommand(poId, request);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return CreatedAtAction(
                    nameof(GetInvoicesByPO),
                    new { poId = result.Data?.PurchaseOrderId },
                    result
                );
            }

            return StatusCode(StatusCodes.Status400BadRequest, result);
        }

        /// <summary>
        /// Add item to invoice
        /// </summary>
        [HttpPost("purchase-invoices/{id}/items")]
        [ProducesResponseType(typeof(ResponseViewModel<InvoiceLineItemResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddInvoiceItem(Guid id, [FromBody] AddInvoiceItemRequest request)
        {
            _logger.LogInformation("Received request to add item to invoice: {InvoiceId}", id);

            var command = new AddInvoiceItemCommand(id, request);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return CreatedAtAction(
                    nameof(GetInvoicesByPO),
                    new { poId = "" }, // You might want to return the specific item endpoint
                    result
                );
            }

            return StatusCode(StatusCodes.Status400BadRequest, result);
        }

        /// <summary>
        /// Update invoice item
        /// </summary>
        [HttpPut("purchase-invoices/{id}/items/{itemId}")]
        [ProducesResponseType(typeof(ResponseViewModel<InvoiceLineItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateInvoiceItem(Guid id, Guid itemId, [FromBody] UpdateInvoiceItemRequest request)
        {
            _logger.LogInformation("Received request to update item {ItemId} in invoice: {InvoiceId}", itemId, id);

            var command = new UpdateInvoiceItemCommand(id, itemId, request);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Delete invoice item
        /// </summary>
        [HttpDelete("purchase-invoices/{id}/items/{itemId}")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteInvoiceItem(Guid id, Guid itemId)
        {
            _logger.LogInformation("Received request to delete item {ItemId} from invoice: {InvoiceId}", itemId, id);

            var command = new DeleteInvoiceItemCommand(id, itemId);
            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}