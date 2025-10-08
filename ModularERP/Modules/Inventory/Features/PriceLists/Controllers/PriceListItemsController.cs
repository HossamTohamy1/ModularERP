using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceList;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceList;
using ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuries_PriceList;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Controllers
{
    [Route("api/PriceList/{priceListId}/items")]
    [ApiController]
    public class PriceListItemsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PriceListItemsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all items in a price list
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseViewModel<List<PriceListItemListDto>>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 404)]
        public async Task<IActionResult> GetAll(
            [FromRoute] Guid priceListId,
            [FromQuery] bool includeInactive = false,
            [FromQuery] DateTime? asOfDate = null)
        {
            var query = new GetPriceListItemsQuery
            {
                PriceListId = priceListId,
                IncludeInactive = includeInactive,
                AsOfDate = asOfDate
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get a specific item in a price list
        /// </summary>
        [HttpGet("{itemId}")]
        [ProducesResponseType(typeof(ResponseViewModel<PriceListItemDto>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 404)]
        public async Task<IActionResult> GetById(
            [FromRoute] Guid priceListId,
            [FromRoute] Guid itemId)
        {
            var query = new GetPriceListItemByIdQuery
            {
                PriceListId = priceListId,
                ItemId = itemId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Add a new item to a price list
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseViewModel<PriceListItemDto>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 400)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 404)]
        public async Task<IActionResult> Create(
            [FromRoute] Guid priceListId,
            [FromBody] CreatePriceListItemDto dto)
        {
            var command = new CreatePriceListItemCommand
            {
                PriceListId = priceListId,
                Item = dto
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Update an existing item in a price list
        /// </summary>
        [HttpPut("{itemId}")]
        [ProducesResponseType(typeof(ResponseViewModel<PriceListItemDto>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 400)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 404)]
        public async Task<IActionResult> Update(
            [FromRoute] Guid priceListId,
            [FromRoute] Guid itemId,
            [FromBody] UpdatePriceListItemDto dto)
        {
            var command = new UpdatePriceListItemCommand
            {
                PriceListId = priceListId,
                ItemId = itemId,
                Item = dto
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Delete an item from a price list
        /// </summary>
        [HttpDelete("{itemId}")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 404)]
        public async Task<IActionResult> Delete(
            [FromRoute] Guid priceListId,
            [FromRoute] Guid itemId)
        {
            var command = new DeletePriceListItemCommand
            {
                PriceListId = priceListId,
                ItemId = itemId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Bulk add multiple items to a price list
        /// </summary>
        [HttpPost("bulk")]
        [ProducesResponseType(typeof(ResponseViewModel<List<PriceListItemDto>>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 400)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 404)]
        public async Task<IActionResult> BulkCreate(
            [FromRoute] Guid priceListId,
            [FromBody] BulkCreatePriceListItemDto dto)
        {
            var command = new BulkCreatePriceListItemsCommand
            {
                PriceListId = priceListId,
                BulkItems = dto
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Bulk update multiple items in a price list
        /// </summary>
        [HttpPut("bulk-update")]
        [ProducesResponseType(typeof(ResponseViewModel<List<PriceListItemDto>>), 200)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 400)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), 404)]
        public async Task<IActionResult> BulkUpdate(
            [FromRoute] Guid priceListId,
            [FromBody] BulkUpdatePriceListItemDto dto)
        {
            var command = new BulkUpdatePriceListItemsCommand
            {
                PriceListId = priceListId,
                BulkItems = dto
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
   