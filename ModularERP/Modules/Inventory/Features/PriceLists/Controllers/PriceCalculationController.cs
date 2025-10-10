using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commands_PriceCalculation;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceCalculation;
using ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceCalculation;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Controllers
{
    [Route("api/price-calculation")]
    [ApiController]
    public class PriceCalculationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PriceCalculationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Calculate final price based on price list rules, discounts, and taxes
        /// </summary>
        [HttpPost("calculate")]
        public async Task<ActionResult<ResponseViewModel<PriceCalculationResultDTO>>> Calculate(
            [FromBody] CalculatePriceCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(ResponseViewModel<PriceCalculationResultDTO>.Success(
                result,
                "Price calculated successfully"));
        }

        /// <summary>
        /// Preview price calculation without creating transaction
        /// </summary>
        [HttpPost("preview")]
        public async Task<ActionResult<ResponseViewModel<PriceCalculationResultDTO>>> Preview(
            [FromBody] PreviewPriceCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(ResponseViewModel<PriceCalculationResultDTO>.Success(
                result,
                "Price preview generated successfully"));
        }

        /// <summary>
        /// Get detailed breakdown of a completed price calculation
        /// </summary>
        [HttpGet("breakdown/{transactionId}")]
        public async Task<ActionResult<ResponseViewModel<PriceBreakdownDTO>>> GetBreakdown(
            Guid transactionId)
        {
            var query = new GetPriceBreakdownQuery { TransactionId = transactionId };
            var result = await _mediator.Send(query);
            return Ok(ResponseViewModel<PriceBreakdownDTO>.Success(
                result,
                "Price breakdown retrieved successfully"));
        }
    }
}
