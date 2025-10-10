using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListAssignment;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListAssignment;
using ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceListAssignment;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Controllers
{
    [ApiController]
    [Route("api/price-list-assignments")]
    public class PriceListAssignmentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PriceListAssignmentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a new price list assignment
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseViewModel<PriceListAssignmentDto>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), 400)]
        public async Task<IActionResult> Create([FromBody] CreatePriceListAssignmentDto dto)
        {
            var command = new CreatePriceListAssignmentCommand { Data = dto };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Get all price list assignments
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseViewModel<List<PriceListAssignmentDto>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllPriceListAssignmentsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get price list assignment by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<PriceListAssignmentDto>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetPriceListAssignmentByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get price list assignments by entity type and ID
        /// </summary>
        [HttpGet("by-entity/{entityType}/{entityId}")]
        [ProducesResponseType(typeof(ResponseViewModel<List<PriceListAssignmentDto>>), 200)]
        public async Task<IActionResult> GetByEntity(PriceListEntityType entityType, Guid entityId)
        {
            var query = new GetPriceListAssignmentsByEntityQuery
            {
                EntityType = entityType,
                EntityId = entityId
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Update an existing price list assignment
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<PriceListAssignmentDto>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), 400)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), 404)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePriceListAssignmentDto dto)
        {
            var command = new UpdatePriceListAssignmentCommand
            {
                Id = id,
                Data = dto
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Delete a price list assignment
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), 404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeletePriceListAssignmentCommand { Id = id };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}