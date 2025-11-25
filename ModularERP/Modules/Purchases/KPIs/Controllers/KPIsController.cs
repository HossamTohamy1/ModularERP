using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.KPIs.DTO;
using ModularERP.Modules.Purchases.KPIs.Qeuries;

namespace ModularERP.Modules.Purchases.KPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KPIsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<KPIsController> _logger;

        public KPIsController(IMediator mediator, ILogger<KPIsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get purchase volume KPI with monthly breakdown
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <param name="startDate">Optional start date (defaults to 12 months ago)</param>
        /// <param name="endDate">Optional end date (defaults to now)</param>
        [HttpGet("purchase-volume")]
        [ProducesResponseType(typeof(ResponseViewModel<PurchaseVolumeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseViewModel<PurchaseVolumeDto>>> GetPurchaseVolume(
            [FromQuery] Guid companyId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            _logger.LogInformation("GetPurchaseVolume endpoint called for CompanyId: {CompanyId}", companyId);

            var query = new GetPurchaseVolumeQuery
            {
                CompanyId = companyId,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get payment trends including status breakdown and monthly trends
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <param name="startDate">Optional start date (defaults to 12 months ago)</param>
        /// <param name="endDate">Optional end date (defaults to now)</param>
        [HttpGet("payment-trends")]
        [ProducesResponseType(typeof(ResponseViewModel<PaymentTrendsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseViewModel<PaymentTrendsDto>>> GetPaymentTrends(
            [FromQuery] Guid companyId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            _logger.LogInformation("GetPaymentTrends endpoint called for CompanyId: {CompanyId}", companyId);

            var query = new GetPaymentTrendsQuery
            {
                CompanyId = companyId,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get supplier performance with top suppliers by purchase volume
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <param name="startDate">Optional start date (defaults to 12 months ago)</param>
        /// <param name="endDate">Optional end date (defaults to now)</param>
        /// <param name="topCount">Number of top suppliers to return (default: 10, max: 100)</param>
        [HttpGet("supplier-performance")]
        [ProducesResponseType(typeof(ResponseViewModel<SupplierPerformanceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseViewModel<SupplierPerformanceDto>>> GetSupplierPerformance(
            [FromQuery] Guid companyId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int topCount = 10)
        {
            _logger.LogInformation("GetSupplierPerformance endpoint called for CompanyId: {CompanyId}", companyId);

            var query = new GetSupplierPerformanceQuery
            {
                CompanyId = companyId,
                StartDate = startDate,
                EndDate = endDate,
                TopCount = topCount
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}