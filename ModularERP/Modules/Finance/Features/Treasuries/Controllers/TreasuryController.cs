using MediatR;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Treasuries.Commands;
using ModularERP.Modules.Finance.Features.Treasuries.Queries;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;

namespace ModularERP.Modules.Finance.Features.Treasuries.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TreasuryController : WalletPermissionsControllerBase
    {
        protected override string WalletType => "Treasury";
        public TreasuryController(IMediator mediator) : base(mediator) { }


        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<IEnumerable<TreasuryListDto>>>> GetAllTreasuries(
            [FromQuery] Guid? companyId = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetAllTreasuriesQuery
            {
                CompanyId = companyId,
                SearchTerm = searchTerm,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return StatusCode(500, result);
            }

            return Ok(result);
        }


        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ResponseViewModel<TreasuryDto>>> GetTreasuryById(Guid id)
        {
            var query = new GetTreasuryByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                if (result.FinanceErrorCode == Common.Enum.Finance_Enum.FinanceErrorCode.TreasuryNotFound)
                {
                    return NotFound(result);
                }
                return StatusCode(500, result);
            }

            return Ok(result);
        }


        [HttpPost]
        public async Task<ActionResult<ResponseViewModel<TreasuryCreatedDto>>> CreateTreasury([FromBody] CreateTreasuryDto createTreasuryDto)
        {
            var command = new CreateTreasuryCommand { Treasury = createTreasuryDto };
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                if (result.FinanceErrorCode == Common.Enum.Finance_Enum.FinanceErrorCode.TreasuryAlreadyExists)
                {
                    return Conflict(result);
                }
                return StatusCode(500, result);
            }

            return CreatedAtAction(
                nameof(GetTreasuryById),
                new { id = result.Data.Id },
                result);
        }


        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> UpdateTreasury(Guid id, [FromBody] UpdateTreasuryDto updateTreasuryDto)
        {
            if (id != updateTreasuryDto.Id)
            {
                return BadRequest(ResponseViewModel<bool>.Error(
                    "Treasury ID in URL does not match the ID in request body",
                    Common.Enum.Finance_Enum.FinanceErrorCode.ValidationError));
            }

            var command = new UpdateTreasuryCommand { Treasury = updateTreasuryDto };
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                if (result.FinanceErrorCode == Common.Enum.Finance_Enum.FinanceErrorCode.TreasuryNotFound)
                {
                    return NotFound(result);
                }
                if (result.FinanceErrorCode == Common.Enum.Finance_Enum.FinanceErrorCode.TreasuryAlreadyExists)
                {
                    return Conflict(result);
                }
                return StatusCode(500, result);
            }

            return Ok(result);
        }


        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> DeleteTreasury(Guid id)
        {
            var command = new DeleteTreasuryCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                if (result.FinanceErrorCode == Common.Enum.Finance_Enum.FinanceErrorCode.TreasuryNotFound)
                {
                    return NotFound(result);
                }
                if (result.FinanceErrorCode == Common.Enum.Finance_Enum.FinanceErrorCode.TreasuryHasVouchers)
                {
                    return BadRequest(result);
                }
                return StatusCode(500, result);
            }

            return Ok(result);
        }

        [HttpGet("by-company/{companyId:guid}")]
        public async Task<ActionResult<ResponseViewModel<IEnumerable<TreasuryListDto>>>> GetTreasuriesByCompany(
              Guid companyId,
              [FromQuery] int pageNumber = 1,
              [FromQuery] int pageSize = 10)
        {
            var query = new GetTreasuriesByCompanyQuery
            {
                CompanyId = companyId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return result.IsSuccess ? Ok(result) : StatusCode(500, result);
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<ResponseViewModel<TreasuryStatisticsDto>>> GetTreasuryStatistics([FromQuery] Guid? companyId = null)
        {
            var query = new GetTreasuryStatisticsQuery { CompanyId = companyId };
            var result = await _mediator.Send(query);
            return result.IsSuccess ? Ok(result) : StatusCode(500, result);
        }

   
    }
}
