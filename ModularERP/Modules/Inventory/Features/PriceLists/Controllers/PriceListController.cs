using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListItems;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;
using ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceListItems;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceListController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PriceListController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all price lists with filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PriceListFilterDto filter)
        {
            var query = new GetAllPriceListsQuery(filter);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get price list by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetPriceListByIdQuery(id);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get price lists assigned to a specific customer
        /// </summary>
        [HttpGet("by-customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(Guid customerId)
        {
            var query = new GetPriceListsByCustomerQuery(customerId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get price lists assigned to a specific supplier
        /// </summary>
        [HttpGet("by-supplier/{supplierId}")]
        public async Task<IActionResult> GetBySupplier(Guid supplierId)
        {
            var query = new GetPriceListsBySupplierQuery(supplierId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Create a new price list
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePriceListDto dto)
        {
            var command = new CreatePriceListCommand(dto);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
        }

        /// <summary>
        /// Update an existing price list
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePriceListDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID mismatch");
            }

            var command = new UpdatePriceListCommand(dto);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Delete a price list
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeletePriceListCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Clone an existing price list
        /// </summary>
        [HttpPost("{id}/clone")]
        public async Task<IActionResult> Clone(Guid id, [FromBody] ClonePriceListDto dto)
        {
            if (id != dto.SourcePriceListId)
            {
                return BadRequest("ID mismatch");
            }

            var command = new ClonePriceListCommand(dto);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
        }

        /// <summary>
        /// Activate a price list
        /// </summary>
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(Guid id)
        {
            var command = new ActivatePriceListCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Deactivate a price list
        /// </summary>
        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var command = new DeactivatePriceListCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Export price list to CSV
        /// </summary>
        [HttpGet("export/{id}")]
        public async Task<IActionResult> Export(Guid id)
        {
            var command = new ExportPriceListCommand(id);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return File(result.Data, "text/csv", $"PriceList_{id}_{DateTime.UtcNow:yyyyMMdd}.csv");
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Import price list from file
        /// </summary>
        //[HttpPost("import")]
        //public async Task<IActionResult> Import([FromForm] ImportPriceListDto dto)
        //{
        //    var command = new ImportPriceListCommand(dto);
        //    var result = await _mediator.Send(command);
        //    return Ok(result);
        //}
    }
}