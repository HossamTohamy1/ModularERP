using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockSnapshot;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockSnapshot;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockSnapshot;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockSnapshotController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public StockSnapshotController(IMediator mediator, ILogger logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get stock snapshot for a stocktaking session
        /// </summary>
        /// <param name="id">Stocktaking ID</param>
        /// <returns>List of product snapshots</returns>
        [HttpGet("{id}/snapshot")]
        [ProducesResponseType(typeof(ResponseViewModel<SnapshotListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSnapshot([FromRoute] Guid id)
        {
            _logger.LogInformation("GET request to fetch snapshot for stocktaking {StocktakingId}", id);

            var query = new GetStockSnapshotQuery { StocktakingId = id };
            var response = await _mediator.Send(query);

            _logger.LogInformation("Successfully retrieved snapshot for stocktaking {StocktakingId}", id);

            return Ok(response);
        }

        /// <summary>
        /// Refresh snapshot with current stock quantities
        /// </summary>
        /// <param name="id">Stocktaking ID</param>
        /// <returns>Refreshed snapshot data</returns>
        [HttpPost("{id}/snapshot/refresh")]
        [ProducesResponseType(typeof(ResponseViewModel<RefreshSnapshotDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshSnapshot([FromRoute] Guid id)
        {
            _logger.LogInformation("POST request to refresh snapshot for stocktaking {StocktakingId}", id);

            var command = new RefreshSnapshotCommand { StocktakingId = id };
            var response = await _mediator.Send(command);

            _logger.LogInformation("Successfully refreshed snapshot for stocktaking {StocktakingId}", id);

            return Ok(response);
        }

        /// <summary>
        /// Compare snapshot quantities with current stock levels
        /// </summary>
        /// <param name="id">Stocktaking ID</param>
        /// <param name="threshold">Drift threshold percentage (default: 5%)</param>
        /// <returns>Comparison results showing drift</returns>
        [HttpGet("{id}/snapshot/compare")]
        [ProducesResponseType(typeof(ResponseViewModel<List<SnapshotComparisonDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CompareSnapshot(
            [FromRoute] Guid id,
            [FromQuery] decimal threshold = 5)
        {
            _logger.LogInformation(
                "GET request to compare snapshot for stocktaking {StocktakingId} with threshold {Threshold}%",
                id, threshold);

            var query = new CompareSnapshotQuery
            {
                StocktakingId = id,
                DriftThreshold = threshold
            };
            var response = await _mediator.Send(query);

            _logger.LogInformation("Successfully compared snapshot for stocktaking {StocktakingId}", id);

            return Ok(response);
        }
    }
}