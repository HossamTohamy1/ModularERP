using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Common.Exceptions;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commands_SupplierPayments;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_SupplierPayments;
using ModularERP.Modules.Purchases.Invoicing.Qeuries.Qeuries_SupplierPayments;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;

namespace ModularERP.Modules.Purchases.Invoicing.Controllers
{
    [ApiController]
    [Route("api/purchases/[controller]")]
    public class SupplierPaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SupplierPaymentsController> _logger;

        public SupplierPaymentsController(
            IMediator mediator,
            ILogger<SupplierPaymentsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
        /// <summary>
        /// Get paginated list of supplier payments
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseViewModel<PagedResult<SupplierPaymentDto>>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<PagedResult<SupplierPaymentDto>>), 400)]
        public async Task<IActionResult> GetSupplierPayments(
            [FromQuery] Guid? supplierId = null,
            [FromQuery] Guid? invoiceId = null,
            [FromQuery] Guid? purchaseOrderId = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                // Use object initializer instead of constructor
                var query = new GetSupplierPaymentsListQuery
                {
                    SupplierId = supplierId,
                    InvoiceId = invoiceId,
                    PurchaseOrderId = purchaseOrderId,
                    Status = status,
                    FromDate = fromDate,
                    ToDate = toDate,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _mediator.Send(query);

                // Check the correct property for success
                if (result != null)
                {
                    return Ok(result);
                }

                return BadRequest(ResponseViewModel<PagedResult<SupplierPaymentDto>>.Error(
                    "No data found",
                    FinanceErrorCode.NotFound,
                    HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier payments list");
                return StatusCode(500, ResponseViewModel<PagedResult<SupplierPaymentDto>>.Error(
                    "An error occurred while retrieving supplier payments",
                    FinanceErrorCode.InternalServerError,
                    HttpContext.TraceIdentifier));
            }
        }
    

/// <summary>
/// Get supplier payment by ID
/// </summary>
[HttpGet("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<SupplierPaymentDto>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<SupplierPaymentDto>), 404)]
        public async Task<IActionResult> GetSupplierPaymentById(Guid id)
        {
            try
            {
                var query = new GetSupplierPaymentByIdQuery(id);
                var result = await _mediator.Send(query);

                if (result.IsSuccess)
                    return Ok(result);

                return NotFound(result);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Supplier payment not found: {PaymentId}", id);
                return NotFound(ResponseViewModel<SupplierPaymentDto>.Error(
                    ex.Message,
                    ex.FinanceErrorCode,
                    HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier payment {PaymentId}", id);
                return StatusCode(500, ResponseViewModel<SupplierPaymentDto>.Error(
                    "An error occurred while retrieving the supplier payment",
                    FinanceErrorCode.InternalServerError,
                    HttpContext.TraceIdentifier));
            }
        }

        /// <summary>
        /// Create a new supplier payment
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseViewModel<SupplierPaymentDto>), 201)]
        [ProducesResponseType(typeof(ResponseViewModel<SupplierPaymentDto>), 400)]
        public async Task<IActionResult> CreateSupplierPayment([FromBody] CreateSupplierPaymentDto dto)
        {
            try
            {
                var command = new CreateSupplierPaymentCommand(dto);
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    return CreatedAtAction(
                        nameof(GetSupplierPaymentById),
                        new { id = result.Data?.Id },
                        result);
                }

                return BadRequest(result);
            }
            catch (Common.Exceptions.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error creating supplier payment");
                return BadRequest(ResponseViewModel<SupplierPaymentDto>.ValidationError(
                    ex.Message,
                    ex.ValidationErrors,
                    HttpContext.TraceIdentifier));
            }
            catch (BusinessLogicException ex)
            {
                _logger.LogWarning(ex, "Business logic error creating supplier payment");
                return BadRequest(ResponseViewModel<SupplierPaymentDto>.Error(
                    ex.Message,
                    ex.FinanceErrorCode,
                    HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier payment");
                return StatusCode(500, ResponseViewModel<SupplierPaymentDto>.Error(
                    "An error occurred while creating the supplier payment",
                    FinanceErrorCode.InternalServerError,
                    HttpContext.TraceIdentifier));
            }
        }

        /// <summary>
        /// Update an existing supplier payment
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<SupplierPaymentDto>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<SupplierPaymentDto>), 404)]
        [ProducesResponseType(typeof(ResponseViewModel<SupplierPaymentDto>), 400)]
        public async Task<IActionResult> UpdateSupplierPayment(Guid id, [FromBody] UpdateSupplierPaymentDto dto)
        {
            try
            {
                // Validate ID match (assuming UpdateSupplierPaymentDto has an Id property)
                // If not, you might need to add it to the DTO
                var command = new UpdateSupplierPaymentCommand(dto);
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Supplier payment not found: {PaymentId}", id);
                return NotFound(ResponseViewModel<SupplierPaymentDto>.Error(
                    ex.Message,
                    ex.FinanceErrorCode,
                    HttpContext.TraceIdentifier));
            }
            catch (Common.Exceptions.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error updating supplier payment {PaymentId}", id);
                return BadRequest(ResponseViewModel<SupplierPaymentDto>.ValidationError(
                    ex.Message,
                    ex.ValidationErrors,
                    HttpContext.TraceIdentifier));
            }
            catch (BusinessLogicException ex)
            {
                _logger.LogWarning(ex, "Business logic error updating supplier payment {PaymentId}", id);
                return BadRequest(ResponseViewModel<SupplierPaymentDto>.Error(
                    ex.Message,
                    ex.FinanceErrorCode,
                    HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier payment {PaymentId}", id);
                return StatusCode(500, ResponseViewModel<SupplierPaymentDto>.Error(
                    "An error occurred while updating the supplier payment",
                    FinanceErrorCode.InternalServerError,
                    HttpContext.TraceIdentifier));
            }
        }

        /// <summary>
        /// Delete a supplier payment (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 404)]
        public async Task<IActionResult> DeleteSupplierPayment(Guid id)
        {
            try
            {
                var command = new DeleteSupplierPaymentCommand(id);
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                    return Ok(result);

                return NotFound(result);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Supplier payment not found: {PaymentId}", id);
                return NotFound(ResponseViewModel<bool>.Error(
                    ex.Message,
                    ex.FinanceErrorCode,
                    HttpContext.TraceIdentifier));
            }
            catch (BusinessLogicException ex)
            {
                _logger.LogWarning(ex, "Business logic error deleting supplier payment {PaymentId}", id);
                return BadRequest(ResponseViewModel<bool>.Error(
                    ex.Message,
                    ex.FinanceErrorCode,
                    HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier payment {PaymentId}", id);
                return StatusCode(500, ResponseViewModel<bool>.Error(
                    "An error occurred while deleting the supplier payment",
                    FinanceErrorCode.InternalServerError,
                    HttpContext.TraceIdentifier));
            }
        }

        /// <summary>
        /// Void a supplier payment
        /// </summary>
        [HttpPost("{id}/void")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 404)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 400)]
        public async Task<IActionResult> VoidSupplierPayment(Guid id, [FromBody] VoidSupplierPaymentDto dto)
        {
            try
            {
                var userId = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53");//GetCurrentUserId();

                if (userId == Guid.Empty)
                {
                    return Unauthorized(ResponseViewModel<bool>.Error(
                        "User not authenticated",
                        FinanceErrorCode.Unauthorized,
                        HttpContext.TraceIdentifier));
                }

                var command = new VoidSupplierPaymentCommand(id, dto.VoidReason, userId);
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Supplier payment not found: {PaymentId}", id);
                return NotFound(ResponseViewModel<bool>.Error(
                    ex.Message,
                    ex.FinanceErrorCode,
                    HttpContext.TraceIdentifier));
            }
            catch (BusinessLogicException ex)
            {
                _logger.LogWarning(ex, "Business logic error voiding supplier payment {PaymentId}", id);
                return BadRequest(ResponseViewModel<bool>.Error(
                    ex.Message,
                    ex.FinanceErrorCode,
                    HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voiding supplier payment {PaymentId}", id);
                return StatusCode(500, ResponseViewModel<bool>.Error(
                    "An error occurred while voiding the supplier payment",
                    FinanceErrorCode.InternalServerError,
                    HttpContext.TraceIdentifier));
            }
        }

        /// <summary>
        /// Get supplier payments by supplier ID
        /// </summary>
        [HttpGet("supplier/{supplierId}")]
        [ProducesResponseType(typeof(ResponseViewModel<PagedResult<SupplierPaymentDto>>), 200)]
        public async Task<IActionResult> GetSupplierPaymentsBySupplierId(
            Guid supplierId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = new GetSupplierPaymentsListQuery
                {
                    SupplierId = supplierId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _mediator.Send(query);

                if (result != null && result.Data.Items != null && result.Data.Items.Any())
                    return Ok(result);

                return BadRequest(ResponseViewModel<PagedResult<SupplierPaymentDto>>.Error(
                    "No supplier payments found",
                    FinanceErrorCode.NotFound,
                    HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for supplier {SupplierId}", supplierId);
                return StatusCode(500, ResponseViewModel<PagedResult<SupplierPaymentDto>>.Error(
                    "An error occurred while retrieving supplier payments",
                    FinanceErrorCode.InternalServerError,
                    HttpContext.TraceIdentifier));
            }
        }


        /// <summary>
        /// Get supplier payments by invoice ID
        /// </summary>
        [HttpGet("invoice/{invoiceId}")]
        [ProducesResponseType(typeof(ResponseViewModel<PagedResult<SupplierPaymentDto>>), 200)]
        public async Task<IActionResult> GetSupplierPaymentsByInvoiceId(
            Guid invoiceId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = new GetSupplierPaymentsListQuery
                {
                    InvoiceId = invoiceId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };


                var result = await _mediator.Send(query);
                if (result != null && result.Data.Items != null && result.Data.Items.Any())
                    return Ok(result);

                return BadRequest(ResponseViewModel<PagedResult<SupplierPaymentDto>>.Error(
                    "No invoice payments found",
                    FinanceErrorCode.NotFound,
                    HttpContext.TraceIdentifier));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for invoice {InvoiceId}", invoiceId);
                return StatusCode(500, ResponseViewModel<PagedResult<SupplierPaymentDto>>.Error(
                    "An error occurred while retrieving invoice payments",
                    FinanceErrorCode.InternalServerError,
                    HttpContext.TraceIdentifier));
            }
        }

        /// <summary>
        /// Get supplier payments by purchase order ID
        /// </summary>
        [HttpGet("purchase-order/{purchaseOrderId}")]
        [ProducesResponseType(typeof(ResponseViewModel<PagedResult<SupplierPaymentDto>>), 200)]
        public async Task<IActionResult> GetSupplierPaymentsByPurchaseOrderId(
            Guid purchaseOrderId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = new GetSupplierPaymentsListQuery
                {

                    PurchaseOrderId = purchaseOrderId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _mediator.Send(query);

                if (result != null && result.Data.Items != null && result.Data.Items.Any())
                    return Ok(result);

                return BadRequest(ResponseViewModel<PagedResult<SupplierPaymentDto>>.Error(
                    "No invoice payments found",
                    FinanceErrorCode.NotFound,
                    HttpContext.TraceIdentifier));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for purchase order {PurchaseOrderId}", purchaseOrderId);
                return StatusCode(500, ResponseViewModel<PagedResult<SupplierPaymentDto>>.Error(
                    "An error occurred while retrieving purchase order payments",
                    FinanceErrorCode.InternalServerError,
                    HttpContext.TraceIdentifier));
            }
        }
    }

    /// <summary>
    /// DTO for voiding supplier payment
    /// </summary>
    public class VoidSupplierPaymentDto
    {
        [Required(ErrorMessage = "Void reason is required")]
        [StringLength(500, ErrorMessage = "Void reason cannot exceed 500 characters")]
        public string VoidReason { get; set; } = string.Empty;
    }
}