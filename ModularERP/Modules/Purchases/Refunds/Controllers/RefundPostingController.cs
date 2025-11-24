using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundItem;
using ModularERP.Modules.Purchases.Refunds.Qeuries.Qeuries_RefundItem;
using ModularERP.Modules.Purchases.Refunds.Queries.Queries_RefundItem;

namespace ModularERP.Modules.Purchases.Refunds.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RefundPostingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RefundPostingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Post refund - يرحل المرتجع وينشئ Debit Note تلقائياً
        /// يحدث الـ Inventory ويقلل الكميات المرتجعة
        /// يربط الـ Debit Note بالـ Supplier Account
        /// </summary>
        [HttpPost("{id}/post")]
        public async Task<IActionResult> PostRefund(Guid id, [FromBody] PostRefundRequest? request)
        {
            var command = new PostRefundCommand
            {
                RefundId = id,
                Notes = request?.Notes
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Get Debit Note linked to a specific refund
        /// </summary>
        [HttpGet("{id}/debit-note")]
        public async Task<IActionResult> GetRefundDebitNote(Guid id)
        {
            var query = new GetRefundDebitNoteQuery { RefundId = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }

    // Request Models
    public record PostRefundRequest
    {
        public string? Notes { get; init; }
    }
}