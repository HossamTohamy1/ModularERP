using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.Commends.Commends_PaymentTerm;
using ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentTerm;
using ModularERP.Modules.Purchases.Payment.Qeuries.Queries_PaymentTerm;

namespace ModularERP.Modules.Purchases.Payment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentTermsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentTermsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all Payment Terms
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseViewModel<List<PaymentTermResponseDto>>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null)
        {
            var query = new GetAllPaymentTermsQuery(isActive);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get Payment Term by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<PaymentTermResponseDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetPaymentTermByIdQuery(id);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Create new Payment Term
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseViewModel<PaymentTermResponseDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreatePaymentTermDto dto)
        {
            var command = new CreatePaymentTermCommand(dto);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Update existing Payment Term
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<PaymentTermResponseDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentTermDto dto)
        {
            var command = new UpdatePaymentTermCommand(id, dto);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Delete Payment Term (Soft Delete)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeletePaymentTermCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
