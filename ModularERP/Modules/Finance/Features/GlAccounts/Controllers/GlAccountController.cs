using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.GlAccounts.Commands;
using ModularERP.Modules.Finance.Features.GlAccounts.DTO;
using ModularERP.Modules.Finance.Features.GlAccounts.Queries;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ModularERP.Modules.Finance.Features.GlAccounts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GlAccountController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger = Log.ForContext<GlAccountController>();

        public GlAccountController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseViewModel<GlAccountResponseDto>>> CreateGlAccount([FromBody] CreateGlAccountDto createDto)
        {
            _logger.Information("Received request to create GLAccount with Code: {Code}", createDto.Code);

            var command = new CreateGlAccountCommand(createDto);
            var result = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetGlAccountById), new { id = result.Data.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseViewModel<GlAccountResponseDto>>> UpdateGlAccount(Guid id, [FromBody] UpdateGlAccountDto updateDto)
        {
            _logger.Information("Received request to update GLAccount with ID: {Id}", id);

            if (id != updateDto.Id)
            {
                _logger.Warning("GLAccount ID mismatch - URL ID: {UrlId}, Body ID: {BodyId}", id, updateDto.Id);
                return BadRequest(ResponseViewModel<GlAccountResponseDto>.Error("ID mismatch", Common.Enum.Finance_Enum.FinanceErrorCode.ValidationError));
            }

            var command = new UpdateGlAccountCommand(updateDto);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> DeleteGlAccount(Guid id)
        {
            _logger.Information("Received request to delete GLAccount with ID: {Id}", id);

            var command = new DeleteGlAccountCommand(id);
            var result = await _mediator.Send(command);

            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseViewModel<GlAccountResponseDto>>> GetGlAccountById(Guid id)
        {
            _logger.Information("Received request to get GLAccount with ID: {Id}", id);

            var query = new GetGlAccountByIdQuery(id);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

   
        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<List<GlAccountResponseDto>>>> GetAllGlAccounts(
            [FromQuery] Guid? companyId = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.Information("Received request to get GLAccounts - CompanyId: {CompanyId}, SearchTerm: {SearchTerm}, Page: {PageNumber}, Size: {PageSize}",
                companyId, searchTerm, pageNumber, pageSize);

            var query = new GetAllGlAccountsQuery(companyId, searchTerm, pageNumber, pageSize);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

  
        [HttpGet("by-company/{companyId}")]
        public async Task<ActionResult<ResponseViewModel<List<GlAccountResponseDto>>>> GetGlAccountsByCompany(
            Guid companyId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.Information("Received request to get GLAccounts by CompanyId: {CompanyId}, Page: {PageNumber}, Size: {PageSize}",
                companyId, pageNumber, pageSize);

            var query = new GetAllGlAccountsQuery(companyId, null, pageNumber, pageSize);
            var result = await _mediator.Send(query);

            return Ok(result);
        }
    }
}

