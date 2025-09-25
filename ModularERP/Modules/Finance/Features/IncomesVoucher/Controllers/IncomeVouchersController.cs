using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Commands;
using ModularERP.Modules.Finance.Features.IncomesVoucher.DTO;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Handlers;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Queries;
using System.Security.Claims;
using System.Text.Json;

namespace ModularERP.Modules.Finance.Features.IncomesVoucher.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncomeVouchersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<IncomeVouchersController> _logger;


        public IncomeVouchersController(IMediator mediator, ILogger<IncomeVouchersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResponseViewModel<IncomeVoucherResponseDto>>> CreateIncomeVoucher(
            [FromForm] string? code,
            [FromForm] DateTime date,
            [FromForm] string currencyCode,
            [FromForm] decimal fxRate,
            [FromForm] decimal amount,
            [FromForm] string description,
            [FromForm] Guid categoryId,
            [FromForm] Guid? recurrenceId,
            [FromForm] string sourceType,
            [FromForm] Guid sourceId,
            [FromForm] string? counterpartyJson,
            [FromForm] string? taxLinesJson,
            [FromForm] List<IFormFile>? attachments
        )
        {
            try
            {
                var userId = GetCurrentUserId();

                var dto = new CreateIncomeVoucherDto
                {
                    Code = code ?? string.Empty,
                    Date = date,
                    CurrencyCode = currencyCode ?? string.Empty,
                    FxRate = fxRate,
                    Amount = amount,
                    Description = description ?? string.Empty,
                    CategoryId = categoryId,
                    RecurrenceId = recurrenceId,

                    // Store JSON as strings
                    SourceJson = $"{{\"Type\":\"{sourceType}\",\"Id\":\"{sourceId}\"}}",
                    CounterpartyJson = counterpartyJson,
                    TaxLinesJson = taxLinesJson,

                    // ✅ Initialize Source object for validation
                    Source = new WalletDto
                    {
                        Type = sourceType ?? string.Empty,
                        Id = sourceId
                    },

                    // For business logic validation
                    SourceId = sourceId,
                    SourceType = sourceType,

                    Attachments = attachments ?? new List<IFormFile>()
                };

                var command = new CreateIncomeVoucherCommand(dto, userId);
                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(
                    nameof(GetIncomeVoucher),
                    new { id = result.Data.Id },
                    result
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateIncomeVoucher controller");
                return StatusCode(500, ResponseViewModel<IncomeVoucherResponseDto>.Error(
                    "An error occurred while processing the request",
                    FinanceErrorCode.InternalServerError));
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseViewModel<IncomeVoucherResponseDto>>> GetIncomeVoucher(Guid id)
        {
            var query = new GetIncomeVoucherByIdQuery(id);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                if (result.FinanceErrorCode == FinanceErrorCode.NotFound)
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResponseViewModel<IncomeVoucherResponseDto>>> UpdateIncomeVoucher(
            Guid id,
            [FromForm] DateTime date,
            [FromForm] string currencyCode,
            [FromForm] decimal fxRate,
            [FromForm] decimal amount,
            [FromForm] string description,
            [FromForm] Guid categoryId,
            [FromForm] Guid? recurrenceId,
            [FromForm] string sourceType,
            [FromForm] Guid sourceId,
            [FromForm] string? counterpartyJson,
            [FromForm] string? taxLinesJson,
            [FromForm] List<IFormFile>? newAttachments,
            [FromForm] string? attachmentsToRemoveJson
        )
        {
            try
            {
                var userId = GetCurrentUserId();

                // Parse attachments to remove
                List<Guid>? attachmentsToRemove = null;
                if (!string.IsNullOrEmpty(attachmentsToRemoveJson))
                {
                    try
                    {
                        attachmentsToRemove = JsonSerializer.Deserialize<List<Guid>>(attachmentsToRemoveJson);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse attachmentsToRemoveJson: {Json}", attachmentsToRemoveJson);
                        return BadRequest(ResponseViewModel<IncomeVoucherResponseDto>.Error(
                            "Invalid format for attachments to remove",
                            FinanceErrorCode.ValidationError));
                    }
                }

                var dto = new UpdateIncomeVoucherDto
                {
                    Id = id,
                    Date = date,
                    CurrencyCode = currencyCode ?? "EGP",
                    FxRate = fxRate,
                    Amount = amount,
                    Description = description ?? string.Empty,
                    CategoryId = categoryId,
                    RecurrenceId = recurrenceId,

                    // Store JSON as strings
                    SourceJson = $"{{\"Type\":\"{sourceType}\",\"Id\":\"{sourceId}\"}}",
                    CounterpartyJson = counterpartyJson,
                    TaxLinesJson = taxLinesJson,

                    // Initialize Source object for validation
                    Source = new WalletDto
                    {
                        Type = sourceType ?? string.Empty,
                        Id = sourceId
                    },

                    // For business logic validation
                    SourceId = sourceId,
                    SourceType = sourceType,

                    NewAttachments = newAttachments ?? new List<IFormFile>(),
                    AttachmentsToRemove = attachmentsToRemove ?? new List<Guid>()
                };

                var command = new UpdateIncomeVoucherCommand(dto, userId);
                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                {
                    if (result.FinanceErrorCode == FinanceErrorCode.NotFound)
                        return NotFound(result);

                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateIncomeVoucher controller");
                return StatusCode(500, ResponseViewModel<IncomeVoucherResponseDto>.Error(
                    "An error occurred while processing the request",
                    FinanceErrorCode.InternalServerError));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> DeleteIncomeVoucher(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var command = new DeleteIncomeVoucherCommand(id, userId);
                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                {
                    if (result.FinanceErrorCode == FinanceErrorCode.NotFound)
                        return NotFound(result);

                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteIncomeVoucher controller");
                return StatusCode(500, ResponseViewModel<bool>.Error(
                    "An error occurred while processing the request",
                    FinanceErrorCode.InternalServerError));
            }
        }


        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}
