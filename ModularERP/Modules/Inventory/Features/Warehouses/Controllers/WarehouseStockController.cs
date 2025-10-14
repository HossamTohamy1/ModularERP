using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Modules.Inventory.Features.Warehouses.Commends.Commends_WarehouseStock;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO.DTO_WarehouseStock;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseStockController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WarehouseStockController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWarehouseStockDto dto)
        {
            var command = new CreateWarehouseStockCommand { Data = dto };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseStockDto dto)
        {
            var command = new UpdateWarehouseStockCommand { Id = id, Data = dto };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteWarehouseStockCommand { Id = id };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetWarehouseStockByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? warehouseId = null,
            [FromQuery] Guid? productId = null,
            [FromQuery] bool? lowStockOnly = null)
        {
            var query = new GetAllWarehouseStocksQuery
            {
                WarehouseId = warehouseId,
                ProductId = productId,
                LowStockOnly = lowStockOnly
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}


