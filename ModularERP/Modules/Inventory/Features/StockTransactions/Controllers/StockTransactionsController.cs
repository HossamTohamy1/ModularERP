using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.StockTransactions.Commends;
using ModularERP.Modules.Inventory.Features.StockTransactions.Qeuries;
using ModularERP.Modules.Inventory.Features.StockTransactions.Commends;
using ModularERP.Modules.Inventory.Features.StockTransactions.DTO;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockTransactionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StockTransactionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all stock transactions with optional filters
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<List<StockTransactionDto>>>> GetAll(
            [FromQuery] Guid? companyId,
            [FromQuery] Guid? warehouseId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var query = new GetAllStockTransactionsQuery
            {
                CompanyId = companyId,
                WarehouseId = warehouseId,
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await _mediator.Send(query);
            return Ok(ResponseViewModel<List<StockTransactionDto>>.Success(result, "Stock transactions retrieved successfully"));
        }

        /// <summary>
        /// Get stock transaction by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseViewModel<StockTransactionDto>>> GetById(Guid id)
        {
            var query = new GetStockTransactionByIdQuery(id);
            var result = await _mediator.Send(query);
            return Ok(ResponseViewModel<StockTransactionDto>.Success(result, "Stock transaction retrieved successfully"));
        }

        /// <summary>
        /// Get stock transactions by product
        /// </summary>
        [HttpGet("by-product/{productId}")]
        public async Task<ActionResult<ResponseViewModel<List<StockTransactionDto>>>> GetByProduct(
            Guid productId,
            [FromQuery] Guid? warehouseId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var query = new GetStockTransactionsByProductQuery(productId)
            {
                WarehouseId = warehouseId,
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await _mediator.Send(query);
            return Ok(ResponseViewModel<List<StockTransactionDto>>.Success(result, "Stock transactions by product retrieved successfully"));
        }

        /// <summary>
        /// Get stock transactions by warehouse
        /// </summary>
        [HttpGet("by-warehouse/{warehouseId}")]
        public async Task<ActionResult<ResponseViewModel<List<StockTransactionDto>>>> GetByWarehouse(
            Guid warehouseId,
            [FromQuery] Guid? productId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var query = new GetStockTransactionsByWarehouseQuery(warehouseId)
            {
                ProductId = productId,
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await _mediator.Send(query);
            return Ok(ResponseViewModel<List<StockTransactionDto>>.Success(result, "Stock transactions by warehouse retrieved successfully"));
        }

        /// <summary>
        /// Get stock transaction summary
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<ResponseViewModel<List<StockTransactionSummaryDto>>>> GetSummary(
            [FromQuery] Guid? companyId,
            [FromQuery] Guid? warehouseId,
            [FromQuery] Guid? productId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var query = new GetStockTransactionSummaryQuery
            {
                CompanyId = companyId,
                WarehouseId = warehouseId,
                ProductId = productId,
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await _mediator.Send(query);
            return Ok(ResponseViewModel<List<StockTransactionSummaryDto>>.Success(result, "Stock transaction summary retrieved successfully"));
        }

        /// <summary>
        /// Create a new stock transaction
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ResponseViewModel<StockTransactionDto>>> Create([FromBody] CreateStockTransactionDto dto)
        {
            var command = new CreateStockTransactionCommand(dto);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ResponseViewModel<StockTransactionDto>.Success(result, "Stock transaction created successfully"));
        }

        /// <summary>
        /// Update an existing stock transaction
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseViewModel<StockTransactionDto>>> Update(Guid id, [FromBody] UpdateStockTransactionDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ResponseViewModel<StockTransactionDto>.Error("ID mismatch", Common.Enum.Finance_Enum.FinanceErrorCode.ValidationError));

            var command = new UpdateStockTransactionCommand(dto);
            var result = await _mediator.Send(command);
            return Ok(ResponseViewModel<StockTransactionDto>.Success(result, "Stock transaction updated successfully"));
        }

        /// <summary>
        /// Delete a stock transaction
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> Delete(Guid id)
        {
            var command = new DeleteStockTransactionCommand(id);
            var result = await _mediator.Send(command);
            return Ok(ResponseViewModel<bool>.Success(result, "Stock transaction deleted successfully"));
        }

        /// <summary>
        /// Bulk create stock transactions
        /// </summary>
        [HttpPost("bulk")]
        public async Task<ActionResult<ResponseViewModel<List<StockTransactionDto>>>> BulkCreate([FromBody] BulkStockTransactionDto dto)
        {
            var command = new BulkCreateStockTransactionsCommand(dto.Transactions);
            var result = await _mediator.Send(command);
            return Ok(ResponseViewModel<List<StockTransactionDto>>.Success(result, $"{result.Count} stock transactions created successfully"));
        }
    }
}
