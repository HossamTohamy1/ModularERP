using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRNPO;
using ModularERP.Modules.Purchases.Goods_Receipt.Qeuries.Qeuires_GRNPO;
using System.Security.Claims;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Controllers
{
    [Route("api/purchase-orders/{poId}")]
    [ApiController]
    public class GRNByPOController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GRNByPOController> _logger;

        public GRNByPOController(IMediator mediator, ILogger<GRNByPOController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all GRNs for a specific Purchase Order
        /// </summary>
        [HttpGet("receipts")]
        public async Task<IActionResult> GetGRNsByPO(
            [FromRoute] Guid poId,
            [FromHeader(Name = "X-Company-ID")] Guid companyId)
        {
            _logger.LogInformation("Getting GRNs for PO {PurchaseOrderId}", poId);

            var query = new GetGRNsByPOQuery
            {
                PurchaseOrderId = poId,
                CompanyId = companyId
            };

            var result = await _mediator.Send(query);

            return Ok(new
            {
                success = true,
                data = result,
                message = "GRNs retrieved successfully"
            });
        }

        /// <summary>
        /// Create GRN directly from Purchase Order
        /// </summary>
        [HttpPost("receive")]
        public async Task<IActionResult> ReceiveFromPO(
            [FromRoute] Guid poId,
            [FromBody] ReceiveFromPOCommand command,
            [FromHeader(Name = "X-Company-ID")] Guid companyId)
        {
            _logger.LogInformation("Creating GRN for PO {PurchaseOrderId}", poId);

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            command.PurchaseOrderId = poId;
            command.CompanyId = companyId;
            command.UserId = userId;

            var result = await _mediator.Send(command);

            return CreatedAtRoute(
                "GetGRNById",
                new { id = result.Id },
                new
                {
                    success = true,
                    data = result,
                    message = "GRN created successfully"
                });
        }

        /// <summary>
        /// Get pending items for receiving from Purchase Order
        /// </summary>
        [HttpGet("pending-items")]
        public async Task<IActionResult> GetPendingItems(
            [FromRoute] Guid poId,
            [FromHeader(Name = "X-Company-ID")] Guid companyId)
        {
            _logger.LogInformation("Getting pending items for PO {PurchaseOrderId}", poId);

            var query = new GetPendingPOItemsQuery
            {
                PurchaseOrderId = poId,
                CompanyId = companyId
            };

            var result = await _mediator.Send(query);

            return Ok(new
            {
                success = true,
                data = result,
                message = "Pending items retrieved successfully"
            });
        }
    }
}