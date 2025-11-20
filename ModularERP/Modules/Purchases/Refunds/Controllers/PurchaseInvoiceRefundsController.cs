using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.Qeuries;
using ModularERP.Modules.Purchases.Refunds.Qeuries.Qeuries_RefundInvoce;

namespace ModularERP.Modules.Purchases.Refunds.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseInvoiceRefundsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PurchaseInvoiceRefundsController> _logger;

        public PurchaseInvoiceRefundsController(IMediator mediator, ILogger<PurchaseInvoiceRefundsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<List<RefundDto>>>> GetRefundsByInvoice(Guid invoiceId)
        {
            _logger.LogInformation("Retrieving refunds for Purchase Invoice: {InvoiceId}", invoiceId);
            var result = await _mediator.Send(new GetRefundsByInvoiceQuery { InvoiceId = invoiceId });
            return Ok(result);
        }

        [HttpPost("return")]
        public async Task<ActionResult<ResponseViewModel<RefundDto>>> CreateRefundFromInvoice(Guid invoiceId, [FromBody] CreateRefundFromInvoiceCommand command)
        {
            if (invoiceId != command.InvoiceId)
            {
                _logger.LogWarning("Invoice ID mismatch: Route={RouteId}, Body={BodyId}", invoiceId, command.InvoiceId);
                return BadRequest(ResponseViewModel<RefundDto>.Error("Invoice ID mismatch", Common.Enum.Finance_Enum.FinanceErrorCode.ValidationError));
            }

            _logger.LogInformation("Creating refund from Purchase Invoice: {InvoiceId}", invoiceId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}