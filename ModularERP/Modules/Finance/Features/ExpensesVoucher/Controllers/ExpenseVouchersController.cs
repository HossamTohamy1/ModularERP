using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<ExpenseVoucherResponseDto>> CreateExpenseVoucher(
            [FromBody] CreateExpenseVoucherDto request)
        {
            var userId = GetCurrentUserId();
            var command = new CreateExpenseVoucherCommand(request, userId);

            try
            {
                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetExpenseVoucher), new { id = result.Id }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseVoucherResponseDto>> GetExpenseVoucher(Guid id)
        {
            var query = new GetExpenseVoucherByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}

