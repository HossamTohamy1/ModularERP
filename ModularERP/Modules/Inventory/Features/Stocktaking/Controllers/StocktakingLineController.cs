using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Line;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Line;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTaking_Line;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Controllers
{
    [Route("api/stocktaking/{stocktakingId}/lines")]
    [ApiController]
    public class StocktakingLineController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<StocktakingLineController> _logger;

        public StocktakingLineController(IMediator mediator, ILogger<StocktakingLineController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseViewModel<DTO.DTO_StockTaking_Line.StocktakingLineDto>>> CreateLine(
            Guid stocktakingId,
            [FromBody] CreateStocktakingLineDto dto)
        {
            _logger.LogInformation("Creating stocktaking line for stocktaking {StocktakingId}", stocktakingId);

            var command = new CreateStocktakingLineCommand
            {
                StocktakingId = stocktakingId,
                ProductId = dto.ProductId,
                PhysicalQty = dto.PhysicalQty,
                Note = dto.Note,
                ImagePath = dto.ImagePath,
                UnitId = dto.UnitId
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation("Stocktaking line created successfully with ID {LineId}", result.Data.Id);

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<List<DTO.DTO_StockTaking_Line.StocktakingLineDto>>>> GetAllLines(Guid stocktakingId)
        {
            _logger.LogInformation("Retrieving all lines for stocktaking {StocktakingId}", stocktakingId);

            var query = new GetStocktakingLinesQuery { StocktakingId = stocktakingId };
            var result = await _mediator.Send(query);

            _logger.LogInformation("Retrieved {Count} lines for stocktaking {StocktakingId}",
                result.Data.Count, stocktakingId);

            return Ok(result);
        }

        [HttpGet("{lineId}")]
        public async Task<ActionResult<ResponseViewModel<DTO.DTO_StockTaking_Line.StocktakingLineDto>>> GetLineById(
            Guid stocktakingId,
            Guid lineId)
        {
            _logger.LogInformation("Retrieving line {LineId} for stocktaking {StocktakingId}",
                lineId, stocktakingId);

            var query = new GetStocktakingLineByIdQuery
            {
                StocktakingId = stocktakingId,
                LineId = lineId
            };

            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpPut("{lineId}")]
        public async Task<ActionResult<ResponseViewModel<DTO.DTO_StockTaking_Line.StocktakingLineDto>>> UpdateLine(
            Guid stocktakingId,
            Guid lineId,
            [FromBody] UpdateStocktakingLineDto dto)
        {
            _logger.LogInformation("Updating line {LineId} for stocktaking {StocktakingId}",
                lineId, stocktakingId);

            var command = new UpdateStocktakingLineCommand
            {
                StocktakingId = stocktakingId,
                LineId = lineId,
                PhysicalQty = dto.PhysicalQty,
                Note = dto.Note,
                ImagePath = dto.ImagePath
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation("Line {LineId} updated successfully", lineId);

            return Ok(result);
        }

        [HttpDelete("{lineId}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> DeleteLine(
            Guid stocktakingId,
            Guid lineId)
        {
            _logger.LogInformation("Deleting line {LineId} from stocktaking {StocktakingId}",
                lineId, stocktakingId);

            var command = new DeleteStocktakingLineCommand
            {
                StocktakingId = stocktakingId,
                LineId = lineId
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation("Line {LineId} deleted successfully", lineId);

            return Ok(result);
        }

        [HttpPost("bulk")]
        public async Task<ActionResult<ResponseViewModel<List<DTO.DTO_StockTaking_Line.StocktakingLineDto>>>> CreateBulkLines(
            Guid stocktakingId,
            [FromBody] List<CreateStocktakingLineDto> dtos)
        {
            _logger.LogInformation("Creating {Count} lines in bulk for stocktaking {StocktakingId}",
                dtos.Count, stocktakingId);

            var command = new CreateBulkStocktakingLinesCommand
            {
                StocktakingId = stocktakingId,
                Lines = dtos
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation("Bulk creation completed: {Count} lines created", result.Data.Count);

            return Ok(result);
        }

        [HttpPost("~/api/stocktaking/{stocktakingId}/add-all-products")]
        public async Task<ActionResult<ResponseViewModel<List<DTO.DTO_StockTaking_Line.StocktakingLineDto>>>> AddAllWarehouseProducts(
            Guid stocktakingId)
        {
            _logger.LogInformation("Adding all warehouse products to stocktaking {StocktakingId}",
                stocktakingId);

            var command = new AddAllWarehouseProductsCommand { StocktakingId = stocktakingId };
            var result = await _mediator.Send(command);

            _logger.LogInformation("Added {Count} products from warehouse", result.Data.Count);

            return Ok(result);
        }
    }
}