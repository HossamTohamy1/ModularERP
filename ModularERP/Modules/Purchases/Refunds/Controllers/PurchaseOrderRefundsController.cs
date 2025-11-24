using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.Qeuries.Qeuries_RefundInvoce;

namespace ModularERP.Modules.Purchases.Refunds.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseOrderRefundsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PurchaseOrderRefundsController> _logger;

        public PurchaseOrderRefundsController(IMediator mediator, ILogger<PurchaseOrderRefundsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<List<RefundDto>>>> GetRefundsByPO(Guid poId)
        {
            _logger.LogInformation("Retrieving refunds for Purchase Order: {PurchaseOrderId}", poId);
            var result = await _mediator.Send(new GetRefundsByPOQuery { PurchaseOrderId = poId });
            return Ok(result);
        }

        [HttpPost("return")]
        public async Task<ActionResult<ResponseViewModel<RefundDto>>> CreateRefundFromPO(Guid poId, [FromBody] CreateRefundFromPOCommand command)
        {
            if (poId != command.PurchaseOrderId)
            {
                _logger.LogWarning("PO ID mismatch: Route={RouteId}, Body={BodyId}", poId, command.PurchaseOrderId);
                return BadRequest(ResponseViewModel<RefundDto>.Error("PO ID mismatch", Common.Enum.Finance_Enum.FinanceErrorCode.ValidationError));
            }

            _logger.LogInformation("Creating refund from Purchase Order: {PurchaseOrderId}", poId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}