using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Commands;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Handlers;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Queries;
using System.Security.Claims;
using System.Text.Json;

namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseVouchersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ExpenseVouchersController> _logger;

        public ExpenseVouchersController(IMediator mediator , ILogger<ExpenseVouchersController> logger)
        {

            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResponseViewModel<ExpenseVoucherResponseDto>>> CreateExpenseVoucher(
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
                var dto = new CreateExpenseVoucherDto
                {
                    Code = code ?? string.Empty,
                    Date = date,
                    CurrencyCode = currencyCode ?? string.Empty,
                    FxRate = fxRate,
                    Amount = amount,
                    Description = description ?? string.Empty,
                    CategoryId = categoryId,
                    RecurrenceId = recurrenceId,
                    
                    SourceJson = $"{{\"Type\":\"{sourceType}\",\"Id\":\"{sourceId}\"}}",
                    CounterpartyJson = counterpartyJson,
                    TaxLinesJson = taxLinesJson,
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

                var command = new CreateExpenseVoucherCommand(dto, userId);
                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(
                    nameof(GetExpenseVoucher),
                    new { id = result.Data.Id },
                    result
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateExpenseVoucher controller");
                return StatusCode(500, ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                    "An error occurred while processing the request",
                    FinanceErrorCode.InternalServerError));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetExpenseVoucher(Guid id)
        {
            var query = new GetExpenseVoucherByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
                return StatusCode(500, "Unexpected null response"); 

            if (!result.IsSuccess)
            {
                return result.FinanceErrorCode switch
                {
                    FinanceErrorCode.NotFound => NotFound(result),
                    FinanceErrorCode.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }

            return Ok(result); 
        }
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResponseViewModel<ExpenseVoucherResponseDto>>> UpdateExpenseVoucher(
            Guid id,
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
            [FromForm] List<IFormFile>? newAttachments,
            [FromForm] List<Guid>? keepAttachmentIds
        )
        {
            try
            {
                var userId = GetCurrentUserId();
                var dto = new UpdateExpenseVoucherDto
                {
                    Id = id,
                    Code = code ?? string.Empty,
                    Date = date,
                    CurrencyCode = currencyCode ?? string.Empty,
                    FxRate = fxRate,
                    Amount = amount,
                    Description = description ?? string.Empty,
                    CategoryId = categoryId,
                    RecurrenceId = recurrenceId,

                    SourceJson = $"{{\"Type\":\"{sourceType}\",\"Id\":\"{sourceId}\"}}",
                    CounterpartyJson = counterpartyJson,
                    TaxLinesJson = taxLinesJson,
                    Source = new WalletDto
                    {
                        Type = sourceType ?? string.Empty,
                        Id = sourceId
                    },
                    // For business logic validation
                    SourceId = sourceId,
                    SourceType = sourceType,
                    NewAttachments = newAttachments ?? new List<IFormFile>(),
                    KeepAttachmentIds = keepAttachmentIds ?? new List<Guid>()
                };

                var command = new UpdateExpenseVoucherCommand(dto, userId);
                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                {
                    return result.FinanceErrorCode switch
                    {
                        FinanceErrorCode.NotFound => NotFound(result),
                        FinanceErrorCode.BusinessLogicError => BadRequest(result),
                        FinanceErrorCode.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateExpenseVoucher controller");
                return StatusCode(500, ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                    "An error occurred while processing the request",
                    FinanceErrorCode.InternalServerError));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseViewModel<string>>> DeleteExpenseVoucher(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var command = new DeleteExpenseVoucherCommand(id, userId);
                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                {
                    return result.FinanceErrorCode switch
                    {
                        FinanceErrorCode.NotFound => NotFound(result),
                        FinanceErrorCode.BusinessLogicError => BadRequest(result),
                        FinanceErrorCode.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteExpenseVoucher controller");
                return StatusCode(500, ResponseViewModel<string>.Error(
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

