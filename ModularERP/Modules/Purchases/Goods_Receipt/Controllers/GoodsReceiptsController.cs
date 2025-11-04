using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.Qeuries.Qeuries_GRN;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoodsReceiptsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GoodsReceiptsController> _logger;

        public GoodsReceiptsController(IMediator mediator, ILogger<GoodsReceiptsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Create a new Goods Receipt Note
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseViewModel<GRNResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateGRN([FromBody] CreateGRNDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("API: Creating new GRN for PO: {PurchaseOrderId}", request.PurchaseOrderId);

            var command = new CreateGRNCommand(request);
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Get all Goods Receipt Notes with optional filters
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseViewModel<List<GRNListItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllGRNs(
            [FromQuery] Guid companyId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] Guid? warehouseId = null,
            [FromQuery] Guid? purchaseOrderId = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("API: Fetching all GRNs for Company: {CompanyId}", companyId);

            var query = new GetAllGRNsQuery(companyId, fromDate, toDate, warehouseId, purchaseOrderId);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Get Goods Receipt Note by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<GRNResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGRNById(Guid id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("API: Fetching GRN with ID: {GRNId}", id);

            var query = new GetGRNByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Update an existing Goods Receipt Note
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<GRNResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateGRN(Guid id, [FromBody] UpdateGRNDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("API: Updating GRN with ID: {GRNId}", id);

            if (id != request.Id)
            {
                return BadRequest(ResponseViewModel<object>.Error(
                    "ID in URL does not match ID in request body",
                    Common.Enum.Finance_Enum.FinanceErrorCode.ValidationError));
            }

            var command = new UpdateGRNCommand(request);
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Delete a Goods Receipt Note
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteGRN(Guid id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("API: Deleting GRN with ID: {GRNId}", id);

            var command = new DeleteGRNCommand(id);
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }
    }
}