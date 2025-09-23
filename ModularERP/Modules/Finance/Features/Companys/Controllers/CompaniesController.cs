using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Companys.Commands;
using ModularERP.Modules.Finance.Features.Companys.DTO;
using ModularERP.Modules.Finance.Features.Companys.Queries;

namespace ModularERP.Modules.Finance.Features.Companys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CompaniesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<List<CompanyResponseDto>>>> GetAllCompanies()
        {
            var query = new GetAllCompaniesQuery();
            var result = await _mediator.Send(query);

            return Ok(result);
        }


        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ResponseViewModel<CompanyResponseDto>>> GetCompanyById(Guid id)
        {
            var query = new GetCompanyByIdQuery(id);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        
        [HttpPost]
        public async Task<ActionResult<ResponseViewModel<CompanyResponseDto>>> CreateCompany([FromBody] CreateCompanyDto createCompanyDto)
        {
            var command = new CreateCompanyCommand(createCompanyDto);
            var result = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetCompanyById),
                new { id = result.Data.Id },
                result
            );
        }


        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ResponseViewModel<CompanyResponseDto>>> UpdateCompany(Guid id, [FromBody] UpdateCompanyDto updateCompanyDto)
        {
            updateCompanyDto.Id = id;
            var command = new UpdateCompanyCommand(updateCompanyDto);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> DeleteCompany(Guid id)
        {
            var command = new DeleteCompanyCommand(id);
            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}

