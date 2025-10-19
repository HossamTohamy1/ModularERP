using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTaking_Header;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocktakingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StocktakingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a new stocktaking
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseViewModel<CreateStocktakingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateStocktakingCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Get all stocktakings with optional filters
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseViewModel<List<StocktakingListDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] Guid? companyId, [FromQuery] Guid? warehouseId, [FromQuery] string? status)
        {
            var query = new GetAllStocktakingQuery
            {
                CompanyId = companyId,
                WarehouseId = warehouseId,
                Status = status
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get stocktaking by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<StocktakingDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetStocktakingByIdQuery(id);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Update stocktaking (only Draft status)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<UpdateStocktakingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStocktakingCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(ResponseViewModel<object>.Error(
                    "ID in URL does not match ID in request body",
                    Common.Enum.Finance_Enum.FinanceErrorCode.ValidationError));
            }

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Delete stocktaking (only Draft status)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteStocktakingCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Get stocktakings by warehouse
        /// </summary>
        [HttpGet("by-warehouse/{warehouseId}")]
        [ProducesResponseType(typeof(ResponseViewModel<List<StocktakingListDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByWarehouse(Guid warehouseId)
        {
            var query = new GetStocktakingByWarehouseQuery(warehouseId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get stocktakings by status
        /// </summary>
        [HttpGet("by-status/{status}")]
        [ProducesResponseType(typeof(ResponseViewModel<List<StocktakingListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByStatus(string status)
        {
            if (!Enum.TryParse<StocktakingStatus>(status, true, out var stocktakingStatus))
            {
                return BadRequest(ResponseViewModel<object>.Error(
                    $"Invalid status value: {status}",
                    Common.Enum.Finance_Enum.FinanceErrorCode.ValidationError));
            }

            var query = new GetStocktakingByStatusQuery(stocktakingStatus);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}