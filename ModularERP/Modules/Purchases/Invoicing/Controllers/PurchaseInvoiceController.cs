using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commands_Invoice;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_Invoice;
using ModularERP.Modules.Purchases.Invoicing.Qeuries.Qeuires_Invoice;

namespace ModularERP.Modules.Purchases.Invoicing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseInvoiceController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PurchaseInvoiceController> _logger;

        public PurchaseInvoiceController(
            IMediator mediator,
            ILogger<PurchaseInvoiceController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseViewModel<PurchaseInvoiceDto>>> CreateInvoice(
            [FromBody] CreatePurchaseInvoiceCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating purchase invoice for PO: {POId}", command.PurchaseOrderId);

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Purchase invoice created successfully: {InvoiceId}", result.Data?.Id);

            return CreatedAtAction(
                nameof(GetInvoiceById),
                new { id = result.Data?.Id },
                result);
        }

        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<List<PurchaseInvoiceDto>>>> GetAllInvoices(
            [FromQuery] GetAllPurchaseInvoicesQuery query,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving all purchase invoices for company: {CompanyId}", query.CompanyId);

            var result = await _mediator.Send(query, cancellationToken);

            _logger.LogInformation("Retrieved {Count} purchase invoices", result.Data?.Count ?? 0);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseViewModel<PurchaseInvoiceDto>>> GetInvoiceById(
            Guid id,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving purchase invoice: {InvoiceId}", id);

            var query = new GetPurchaseInvoiceByIdQuery { Id = id };
            var result = await _mediator.Send(query, cancellationToken);

            _logger.LogInformation("Purchase invoice retrieved successfully: {InvoiceId}", id);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseViewModel<PurchaseInvoiceDto>>> UpdateInvoice(
            Guid id,
            [FromBody] UpdatePurchaseInvoiceCommand command,
            CancellationToken cancellationToken)
        {
            if (id != command.Id)
            {
                _logger.LogWarning("Invoice ID mismatch: URL={UrlId}, Body={BodyId}", id, command.Id);
                return BadRequest(ResponseViewModel<bool>.Error(
                    "Invoice ID in URL does not match ID in request body",
                    Common.Enum.Finance_Enum.FinanceErrorCode.ValidationError));
            }

            _logger.LogInformation("Updating purchase invoice: {InvoiceId}", id);

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Purchase invoice updated successfully: {InvoiceId}", id);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> DeleteInvoice(
            Guid id,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting purchase invoice: {InvoiceId}", id);

            var command = new DeletePurchaseInvoiceCommand { Id = id };
            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Purchase invoice deleted successfully: {InvoiceId}", id);

            return Ok(result);
        }
    }
}