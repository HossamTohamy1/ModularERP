using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.Commends;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.Qeuries;
using ModularERP.Modules.Purchases.Refunds.Qeuries.Qeuries_RefundInvoce;

namespace ModularERP.Modules.Purchases.Refunds.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RefundsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RefundsController> _logger;

        public RefundsController(IMediator mediator, ILogger<RefundsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseViewModel<RefundDto>>> CreateRefund([FromBody] CreateRefundCommand command)
        {
            _logger.LogInformation("Creating new refund for PO: {PurchaseOrderId}", command.PurchaseOrderId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<List<RefundDto>>>> GetAllRefunds([FromQuery] GetAllRefundsQuery query)
        {
            _logger.LogInformation("Retrieving all refunds");
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseViewModel<RefundDto>>> GetRefundById(Guid id)
        {
            _logger.LogInformation("Retrieving refund with ID: {RefundId}", id);
            var result = await _mediator.Send(new GetRefundByIdQuery { Id = id });
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseViewModel<RefundDto>>> UpdateRefund(Guid id, [FromBody] UpdateRefundCommand command)
        {
            if (id != command.Id)
            {
                _logger.LogWarning("Refund ID mismatch: Route={RouteId}, Body={BodyId}", id, command.Id);
                return BadRequest(ResponseViewModel<RefundDto>.Error("ID mismatch", Common.Enum.Finance_Enum.FinanceErrorCode.ValidationError));
            }

            _logger.LogInformation("Updating refund: {RefundId}", id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> DeleteRefund(Guid id)
        {
            _logger.LogInformation("Deleting refund: {RefundId}", id);
            var result = await _mediator.Send(new DeleteRefundCommand { Id = id });
            return Ok(result);
        }
    }
}