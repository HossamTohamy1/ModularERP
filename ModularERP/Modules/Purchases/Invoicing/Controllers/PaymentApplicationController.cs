using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commands_PaymentApplication;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_PaymentApplication;
using ModularERP.Modules.Purchases.Invoicing.Validators.Validators_PaymentApplication;

namespace ModularERP.Modules.Purchases.Invoicing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentApplicationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentApplicationController> _logger;

        public PaymentApplicationController(IMediator mediator, ILogger<PaymentApplicationController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Apply payment to multiple invoices
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <param name="dto">Apply payment details</param>
        /// <returns>Payment application summary</returns>
        [HttpPost("supplier-payments/{id}/apply")]
        [ProducesResponseType(typeof(ResponseViewModel<PaymentApplicationSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApplyPayment(Guid id, [FromBody] ApplyPaymentDto dto)
        {
            _logger.LogInformation("ApplyPayment endpoint called for payment {PaymentId}", id);

            // Validate
            var validator = new ApplyPaymentValidator();
            var validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(ResponseViewModel<object>.ValidationError(
                    "Validation failed",
                    errors));
            }

            var command = new ApplyPaymentCommand(id, dto);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Allocate payment to invoices (alternative endpoint)
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <param name="allocations">Invoice allocations</param>
        /// <returns>Payment application summary</returns>
        [HttpPost("supplier-payments/{id}/allocate")]
        [ProducesResponseType(typeof(ResponseViewModel<PaymentApplicationSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AllocatePayment(Guid id, [FromBody] List<InvoiceAllocationDto> allocations)
        {
            _logger.LogInformation("AllocatePayment endpoint called for payment {PaymentId}", id);

            // Validate each allocation
            var validator = new InvoiceAllocationValidator();
            foreach (var allocation in allocations)
            {
                var validationResult = await validator.ValidateAsync(allocation);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        );

                    return BadRequest(ResponseViewModel<object>.ValidationError(
                        "Validation failed",
                        errors));
                }
            }

            var command = new AllocatePaymentCommand(id, allocations);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Pay a specific invoice
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <param name="dto">Payment details</param>
        /// <returns>Payment application summary</returns>
        [HttpPost("purchase-invoices/{invoiceId}/pay")]
        [ProducesResponseType(typeof(ResponseViewModel<PaymentApplicationSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PayInvoice(Guid invoiceId, [FromBody] PayInvoiceDto dto)
        {
            _logger.LogInformation("PayInvoice endpoint called for invoice {InvoiceId}", invoiceId);

            // Validate
            var validator = new PayInvoiceValidator();
            var validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(ResponseViewModel<object>.ValidationError(
                    "Validation failed",
                    errors));
            }

            var command = new PayInvoiceCommand(invoiceId, dto);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Quick pay to supplier (can be advance or against invoices)
        /// </summary>
        /// <param name="supplierId">Supplier ID</param>
        /// <param name="dto">Quick payment details</param>
        /// <returns>Payment application summary</returns>
        [HttpPost("suppliers/{supplierId}/pay")]
        [ProducesResponseType(typeof(ResponseViewModel<PaymentApplicationSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> QuickPaySupplier(Guid supplierId, [FromBody] QuickPaySupplierDto dto)
        {
            _logger.LogInformation("QuickPaySupplier endpoint called for supplier {SupplierId}", supplierId);

            // Validate
            var validator = new QuickPaySupplierValidator();
            var validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(ResponseViewModel<object>.ValidationError(
                    "Validation failed",
                    errors));
            }

            var command = new QuickPaySupplierCommand(supplierId, dto);
            var result = await _mediator.Send(command);

            return Ok(result);
        }
        /// <summary>
        /// Void an allocation
        /// </summary>
        /// <param name="allocationId">Allocation ID</param>
        /// <param name="dto">Void reason</param>
        /// <returns>Success result</returns>
        [HttpPost("allocations/{allocationId}/void")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VoidAllocation(Guid allocationId, [FromBody] VoidAllocationDto dto)
        {
            _logger.LogInformation("VoidAllocation endpoint called for allocation {AllocationId}", allocationId);

            // Validate
            var validator = new VoidAllocationValidator();
            var validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(ResponseViewModel<object>.ValidationError(
                    "Validation failed",
                    errors));
            }

            var command = new VoidAllocationCommand(allocationId, dto);
            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}

