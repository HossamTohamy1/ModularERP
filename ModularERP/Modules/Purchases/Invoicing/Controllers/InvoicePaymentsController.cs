using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem;
using ModularERP.Modules.Purchases.Invoicing.Qeuries.Qeuires_InvociePayment;

namespace ModularERP.Modules.Purchases.Invoicing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicePaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InvoicePaymentsController> _logger;

        public InvoicePaymentsController(IMediator mediator, ILogger<InvoicePaymentsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all payments for a specific invoice
        /// </summary>
        /// <param name="id">Invoice ID</param>
        /// <returns>List of payments with payment details</returns>
        [HttpGet("{id}/payments")]
        [ProducesResponseType(typeof(GetInvoicePaymentsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetInvoicePayments([FromRoute] Guid id)
        {
            _logger.LogInformation("GetInvoicePayments endpoint called for invoice {InvoiceId}", id);

            var query = new GetInvoicePaymentsQuery { InvoiceId = id };
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Get the balance details for a specific invoice
        /// </summary>
        /// <param name="id">Invoice ID</param>
        /// <returns>Invoice balance information including amounts, payments, and overdue status</returns>
        [HttpGet("{id}/balance")]
        [ProducesResponseType(typeof(GetInvoiceBalanceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetInvoiceBalance([FromRoute] Guid id)
        {
            _logger.LogInformation("GetInvoiceBalance endpoint called for invoice {InvoiceId}", id);

            var query = new GetInvoiceBalanceQuery { InvoiceId = id };
            var result = await _mediator.Send(query);

            return Ok(result);
        }
    }
}