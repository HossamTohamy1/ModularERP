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
        private readonly ILogger<StockSnapshotController> _logger;

        public StockSnapshotController(IMediator mediator, ILogger<StockSnapshotController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Create initial snapshot when starting stocktaking
        /// </summary>
        /// <param name="id">Stocktaking ID</param>
        /// <returns>Created snapshot data</returns>
        [HttpPost("{id}/snapshot")]
        [ProducesResponseType(typeof(ResponseViewModel<SnapshotListDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSnapshot([FromRoute] Guid id)
        {
            _logger.LogInformation("POST request to create snapshot for stocktaking {StocktakingId}", id);

            var command = new CreateSnapshotCommand { StocktakingId = id };
            var response = await _mediator.Send(command);

            _logger.LogInformation("Successfully created snapshot for stocktaking {StocktakingId}", id);
            return CreatedAtAction(nameof(GetSnapshot), new { id }, response);
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
        /// Get snapshot for specific product in stocktaking
        /// </summary>
        /// <param name="id">Stocktaking ID</param>
        /// <param name="productId">Product ID</param>
        /// <returns>Single product snapshot</returns>
        [HttpGet("{id}/snapshot/product/{productId}")]
        [ProducesResponseType(typeof(ResponseViewModel<StockSnapshotDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductSnapshot(
            [FromRoute] Guid id,
            [FromRoute] Guid productId)
        {
            _logger.LogInformation(
                "GET request to fetch snapshot for product {ProductId} in stocktaking {StocktakingId}",
                productId, id);

            var query = new GetProductSnapshotQuery
            {
                StocktakingId = id,
                ProductId = productId
            };
            var response = await _mediator.Send(query);

            _logger.LogInformation("Successfully retrieved product snapshot");
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

        /// <summary>
        /// Get products that exceed drift threshold
        /// </summary>
        /// <param name="id">Stocktaking ID</param>
        /// <param name="threshold">Drift threshold percentage (default: 5%)</param>
        /// <returns>List of products exceeding threshold</returns>
        [HttpGet("{id}/snapshot/exceptions")]
        [ProducesResponseType(typeof(ResponseViewModel<List<SnapshotComparisonDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSnapshotExceptions(
            [FromRoute] Guid id,
            [FromQuery] decimal threshold = 5)
        {
            _logger.LogInformation(
                "GET request to fetch snapshot exceptions for stocktaking {StocktakingId} with threshold {Threshold}%",
                id, threshold);

            var query = new GetSnapshotExceptionsQuery
            {
                StocktakingId = id,
                DriftThreshold = threshold
            };
            var response = await _mediator.Send(query);

            _logger.LogInformation("Successfully retrieved snapshot exceptions for stocktaking {StocktakingId}", id);
            return Ok(response);
        }

        /// <summary>
        /// Delete snapshot (only for Draft status)
        /// </summary>
        /// <param name="id">Stocktaking ID</param>
        /// <returns>Success confirmation</returns>
        [HttpDelete("{id}/snapshot")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteSnapshot([FromRoute] Guid id)
        {
            _logger.LogInformation("DELETE request to remove snapshot for stocktaking {StocktakingId}", id);

            var command = new DeleteSnapshotCommand { StocktakingId = id };
            var response = await _mediator.Send(command);

            _logger.LogInformation("Successfully deleted snapshot for stocktaking {StocktakingId}", id);
            return Ok(response);
        }

        /// <summary>
        /// Export snapshot data to CSV/Excel
        /// </summary>
        /// <param name="id">Stocktaking ID</param>
        /// <param name="format">Export format (csv or excel)</param>
        /// <returns>File download</returns>
        [HttpGet("{id}/snapshot/export")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportSnapshot(
            [FromRoute] Guid id,
            [FromQuery] string format = "csv")
        {
            _logger.LogInformation(
                "GET request to export snapshot for stocktaking {StocktakingId} in {Format} format",
                id, format);

            var query = new ExportSnapshotQuery
            {
                StocktakingId = id,
                Format = format
            };
            var response = await _mediator.Send(query);

            var contentType = format.ToLower() == "excel"
                ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                : "text/csv";
            var fileExtension = format.ToLower() == "excel" ? "xlsx" : "csv";

            _logger.LogInformation("Successfully exported snapshot for stocktaking {StocktakingId}", id);
            return File(response.Data, contentType, $"snapshot_{id}.{fileExtension}");
        }

        /// <summary>
        /// Get snapshot statistics summary
        /// </summary>
        /// <param name="id">Stocktaking ID</param>
        /// <returns>Snapshot statistics</returns>
        [HttpGet("{id}/snapshot/statistics")]
        [ProducesResponseType(typeof(ResponseViewModel<SnapshotStatisticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSnapshotStatistics([FromRoute] Guid id)
        {
            _logger.LogInformation(
                "GET request to fetch snapshot statistics for stocktaking {StocktakingId}", id);

            var query = new GetSnapshotStatisticsQuery { StocktakingId = id };
            var response = await _mediator.Send(query);

            _logger.LogInformation("Successfully retrieved snapshot statistics for stocktaking {StocktakingId}", id);
            return Ok(response);
        }
    }
}