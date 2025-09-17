using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Commands;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Queries;
using System.Security.Claims;

namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseVouchersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ExpenseVouchersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseViewModel<ExpenseVoucherResponseDto>>> CreateExpenseVoucher(
            [FromBody] CreateExpenseVoucherDto request)
        {
            var userId = GetCurrentUserId();
            var command = new CreateExpenseVoucherCommand(request, userId);

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetExpenseVoucher), new { id = result.Data.Id }, result);

            if (result.FinanceErrorCode == FinanceErrorCode.ValidationError)
                return BadRequest(result);

            return BadRequest(result);
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

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}

