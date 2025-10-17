using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ProductStats;
using ModularERP.Modules.Inventory.Features.Products.Qeuries.Qeuries_ProductStats;

namespace ModularERP.Modules.Inventory.Features.Products.Controllers
{
    [Route("api/products/{productId}/stats")]
    [ApiController]
    public class ProductStatsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductStatsController(IMediator mediator, IHttpContextAccessor httpContextAccessor)
        {
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        private Guid GetCompanyId()
        {
            var companyIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("company_id")?.Value;

            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                throw new UnauthorizedAccessException("Company ID not found in token");
            }

            return companyId;
        }

        /// <summary>
        /// Get complete product statistics
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<ProductStatsDto>>> GetProductStats(
            [FromRoute] Guid productId,
            CancellationToken cancellationToken)
        {
            var query = new GetProductStatsQuery
            {
                ProductId = productId,
                CompanyId = Guid.Parse("92f798f0-ae62-41b4-aad5-80ae06f8cb1a") 
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(ResponseViewModel<ProductStatsDto>.Success(result, "Product statistics retrieved successfully"));
        }

        /// <summary>
        /// Refresh product statistics by recalculating from transactions
        /// </summary>
        [HttpGet("refresh")]
        [HttpPost("refresh")]
        public async Task<ActionResult<ResponseViewModel<RefreshStatsResultDto>>> RefreshProductStats(
            [FromRoute] Guid productId,
            CancellationToken cancellationToken)
        {
            var command = new RefreshProductStatsCommand
            {
                ProductId = productId,
                CompanyId = Guid.Parse("92f798f0-ae62-41b4-aad5-80ae06f8cb1a")
            };

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(ResponseViewModel<RefreshStatsResultDto>.Success(result, "Product statistics refreshed successfully"));
        }

        /// <summary>
        /// Get on-hand stock for a product
        /// </summary>
        [HttpGet("on-hand-stock")]
        public async Task<ActionResult<ResponseViewModel<OnHandStockDto>>> GetOnHandStock(
            [FromRoute] Guid productId,
            CancellationToken cancellationToken)
        {
            var query = new GetOnHandStockQuery
            {
                ProductId = productId,
                CompanyId = Guid.Parse("92f798f0-ae62-41b4-aad5-80ae06f8cb1a")
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(ResponseViewModel<OnHandStockDto>.Success(result, "On-hand stock retrieved successfully"));
        }

        /// <summary>
        /// Get average unit cost for a product
        /// </summary>
        [HttpGet("average-cost")]
        public async Task<ActionResult<ResponseViewModel<AverageCostDto>>> GetAverageCost(
            [FromRoute] Guid productId,
            CancellationToken cancellationToken)
        {
            var query = new GetAverageCostQuery
            {
                ProductId = productId,
                CompanyId = Guid.Parse("92f798f0-ae62-41b4-aad5-80ae06f8cb1a")
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(ResponseViewModel<AverageCostDto>.Success(result, "Average cost retrieved successfully"));
        }

        /// <summary>
        /// Get sales statistics for a product
        /// </summary>
        [HttpGet("sales-stats")]
        public async Task<ActionResult<ResponseViewModel<SalesStatsDto>>> GetSalesStats(
            [FromRoute] Guid productId,
            CancellationToken cancellationToken)
        {
            var query = new GetSalesStatsQuery
            {
                ProductId = productId,
                CompanyId = Guid.Parse("92f798f0-ae62-41b4-aad5-80ae06f8cb1a")
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(ResponseViewModel<SalesStatsDto>.Success(result, "Sales statistics retrieved successfully"));
        }
    }
}