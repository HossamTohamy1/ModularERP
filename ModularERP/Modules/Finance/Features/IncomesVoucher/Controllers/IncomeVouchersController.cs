using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Commands;
using ModularERP.Modules.Finance.Features.IncomesVoucher.DTO;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Queries;
using System.Security.Claims;

namespace ModularERP.Modules.Finance.Features.IncomesVoucher.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncomeVouchersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public IncomeVouchersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseViewModel<IncomeVoucherResponseDto>>> CreateIncomeVoucher(
           [FromBody] CreateIncomeVoucherDto request)
        {
            var userId = GetCurrentUserId();
            var command = new CreateIncomeVoucherCommand(request, userId);

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                // Validation errors
                if (result.ValidationErrors != null && result.ValidationErrors.Any())
                    return BadRequest(result);

                // Forbidden
                //if (result.ErrorCode == FinanceErrorCode.Unauthorized)
                //    return Forbid();

                // Business logic errors
                return BadRequest(result);
            }

            // Success → return CreatedAtAction
            return CreatedAtAction(
                nameof(GetIncomeVoucher),
                new { id = result.Data.Id },
                result
            );
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


        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}
