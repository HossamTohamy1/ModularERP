using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.TaxManagement.Commends;
using ModularERP.Modules.Inventory.Features.TaxManagement.DTO;
using ModularERP.Modules.Inventory.Features.TaxManagement.Qeuries;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxComponentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaxComponentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<List<TaxComponentDto>>>> GetAll()
        {
            var result = await _mediator.Send(new GetAllTaxComponentsQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseViewModel<TaxComponentDto>>> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetTaxComponentByIdQuery { Id = id });
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ResponseViewModel<TaxComponentDto>>> Create([FromBody] CreateTaxComponentCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseViewModel<TaxComponentDto>>> Update(Guid id, [FromBody] UpdateTaxComponentCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteTaxComponentCommand { Id = id });
            return Ok(result);
        }
    }
}

