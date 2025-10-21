using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commands_StockTraking_WorkFlow;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_WorkFlow;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTraking_WorkFlow;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocktakingWorkFlowController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public StocktakingWorkFlowController(IMediator mediator, ILogger logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Create a new stocktaking session
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<ResponseViewModel<CreateStocktakingDto>>> Create(
            [FromBody] CreateStocktakingCommand command,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("POST: Create Stocktaking");
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Update stocktaking details (draft only)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseViewModel<UpdateStocktakingDto>>> Update(
            Guid id,
            [FromBody] UpdateStocktakingCommand command,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("PUT: Update Stocktaking {StocktakingId}", id);
            command.StocktakingId = id;
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Delete stocktaking (draft only)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> Delete(
            Guid id,
            [FromQuery] Guid companyId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("DELETE: Delete Stocktaking {StocktakingId}", id);
            var command = new DeleteStocktakingCommand { StocktakingId = id, CompanyId = companyId };
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get detailed view of stocktaking
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseViewModel<StocktakingDetailDto>>> GetDetail(
            Guid id,
            [FromQuery] Guid companyId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("GET: Stocktaking Detail {StocktakingId}", id);
            var query = new GetStocktakingDetailQuery { StocktakingId = id, CompanyId = companyId };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get list of all stocktakings with filters
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<List<StocktakingListDto>>>> GetAll(
            [FromQuery] Guid companyId,
            [FromQuery] Guid? warehouseId = null,
            [FromQuery] string status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("GET: List Stocktakings for Company {CompanyId}", companyId);

            StocktakingStatus? parsedStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<StocktakingStatus>(status, true, out var enumStatus))
            {
                parsedStatus = enumStatus;
            }

            var query = new GetAllStocktakingsQuery
            {
                CompanyId = companyId,
                WarehouseId = warehouseId,
                Status = parsedStatus,
                FromDate = fromDate,
                ToDate = toDate,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Start stocktaking session (move from Draft to Counting)
        /// </summary>
        [HttpPost("{id}/start")]
        public async Task<ActionResult<ResponseViewModel<StartStocktakingDto>>> Start(
            Guid id,
            [FromQuery] Guid companyId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("POST: Start Stocktaking {StocktakingId}", id);
            var userId = GetCurrentUserId();
            var command = new StartStocktakingCommand
            {
                StocktakingId = id,
                CompanyId = companyId,
                UserId = userId
            };
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Move stocktaking to review (move from Counting to Review)
        /// </summary>
        [HttpPost("{id}/review")]
        public async Task<ActionResult<ResponseViewModel<ReviewStocktakingDto>>> Review(
            Guid id,
            [FromQuery] Guid companyId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("POST: Review Stocktaking {StocktakingId}", id);
            var userId = GetCurrentUserId();
            var command = new ReviewStocktakingCommand
            {
                StocktakingId = id,
                CompanyId = companyId,
                UserId = userId
            };
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Post stocktaking and create adjustments (move from Review to Posted)
        /// </summary>
        [HttpPost("{id}/post")]
        public async Task<ActionResult<ResponseViewModel<PostStocktakingDto>>> Post(
            Guid id,
            [FromQuery] Guid companyId,
            [FromQuery] bool forcePost = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("POST: Post Stocktaking {StocktakingId}", id);
            var userId = GetCurrentUserId();
            var command = new PostStocktakingCommand
            {
                StocktakingId = id,
                CompanyId = companyId,
                UserId = userId,
                ForcePost = forcePost
            };
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get variance summary for stocktaking
        /// </summary>
        [HttpGet("{id}/variance-summary")]
        public async Task<ActionResult<ResponseViewModel<VarianceSummaryDto>>> GetVarianceSummary(
            Guid id,
            [FromQuery] Guid companyId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("GET: Variance Summary for Stocktaking {StocktakingId}", id);
            var query = new GetStocktakingVarianceSummaryQuery { StocktakingId = id, CompanyId = companyId };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get preview of adjustments before posting
        /// </summary>
        [HttpGet("{id}/preview-adjustments")]
        public async Task<ActionResult<ResponseViewModel<PreviewAdjustmentsDto>>> GetPreviewAdjustments(
            Guid id,
            [FromQuery] Guid companyId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("GET: Preview Adjustments for Stocktaking {StocktakingId}", id);
            var query = new GetPreviewAdjustmentsQuery { StocktakingId = id, CompanyId = companyId };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}