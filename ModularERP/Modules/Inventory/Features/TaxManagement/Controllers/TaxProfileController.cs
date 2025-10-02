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
    public class TaxProfileController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaxProfileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<List<TaxProfileDto>>>> GetAll()
        {
            var result = await _mediator.Send(new GetAllTaxProfilesQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseViewModel<TaxProfileDetailDto>>> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetTaxProfileByIdQuery { Id = id });
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ResponseViewModel<TaxProfileDto>>> Create([FromBody] CreateTaxProfileCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseViewModel<TaxProfileDto>>> Update(Guid id, [FromBody] UpdateTaxProfileCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteTaxProfileCommand { Id = id });
            return Ok(result);
        }

        [HttpPost("{id}/components")]
        public async Task<ActionResult<ResponseViewModel<bool>>> AddComponent(Guid id, [FromBody] AddTaxComponentToProfileCommand command)
        {
            command.TaxProfileId = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}/components/{componentId}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> RemoveComponent(Guid id, Guid componentId)
        {
            var result = await _mediator.Send(new RemoveTaxComponentFromProfileCommand
            {
                TaxProfileId = id,
                TaxComponentId = componentId
            });
            return Ok(result);
        }
    }
}

