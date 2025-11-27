using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.Commends.Commends_PaymentMethod;
using ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentMethod;
using ModularERP.Modules.Purchases.Payment.Qeuries.Quries_PaymentMethod;

namespace ModularERP.Modules.Purchases.Payment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentMethodsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentMethodsController> _logger;

        public PaymentMethodsController(IMediator mediator, ILogger<PaymentMethodsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all payment methods
        /// </summary>
        /// <param name="isActive">Filter by active status (optional)</param>
        /// <returns>List of payment methods</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseViewModel<List<PaymentMethodDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] bool? isActive)
        {
            _logger.LogInformation("GET /api/payment-methods - IsActive: {IsActive}", isActive);

            var query = new GetAllPaymentMethodsQuery { IsActive = isActive };
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Get active payment methods only
        /// </summary>
        /// <returns>List of active payment methods</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(ResponseViewModel<List<PaymentMethodDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetActive()
        {
            _logger.LogInformation("GET /api/payment-methods/active");

            var query = new GetActivePaymentMethodsQuery();
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Get payment method by ID
        /// </summary>
        /// <param name="id">Payment method ID</param>
        /// <returns>Payment method details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ResponseViewModel<PaymentMethodDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            _logger.LogInformation("GET /api/payment-methods/{Id}", id);

            var query = new GetPaymentMethodByIdQuery(id);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Create new payment method
        /// </summary>
        /// <param name="command">Payment method data</param>
        /// <returns>Created payment method</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseViewModel<PaymentMethodDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreatePaymentMethodCommand command)
        {
            _logger.LogInformation("POST /api/payment-methods - Name: {Name}, Code: {Code}", command.Name, command.Code);

            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Update existing payment method
        /// </summary>
        /// <param name="id">Payment method ID</param>
        /// <param name="command">Updated payment method data</param>
        /// <returns>Updated payment method</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ResponseViewModel<PaymentMethodDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentMethodCommand command)
        {
            _logger.LogInformation("PUT /api/payment-methods/{Id} - Name: {Name}, Code: {Code}", id, command.Name, command.Code);

            if (id != command.Id)
            {
                _logger.LogWarning("ID mismatch - URL ID: {UrlId}, Body ID: {BodyId}", id, command.Id);
                return BadRequest(ResponseViewModel<bool>.Fail(
                    "ID in URL does not match ID in request body",
                    Common.Enum.Finance_Enum.FinanceErrorCode.ValidationError));
            }

            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Delete payment method (soft delete)
        /// </summary>
        /// <param name="id">Payment method ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("DELETE /api/payment-methods/{Id}", id);

            var command = new DeletePaymentMethodCommand(id);
            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}
