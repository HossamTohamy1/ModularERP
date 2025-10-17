using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ProductTimeline;
using ModularERP.Modules.Inventory.Features.Products.Qeuries.Qeuries_Product;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Category;

namespace ModularERP.Modules.Inventory.Features.Products.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductTimelineController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductTimelineController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{productId}/timeline")]
        [ProducesResponseType(typeof(ResponseViewModel<PaginatedResult<ProductTimelineDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductTimeline(
            [FromRoute] Guid productId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetProductTimelineQuery
            {
                ProductId = productId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{productId}/timeline/by-date-range")]
        [ProducesResponseType(typeof(ResponseViewModel<IEnumerable<ProductTimelineDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductTimelineByDateRange(
            [FromRoute] Guid productId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var query = new GetProductTimelineByDateRangeQuery
            {
                ProductId = productId,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }

}
