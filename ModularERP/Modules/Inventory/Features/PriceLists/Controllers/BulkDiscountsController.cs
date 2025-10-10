using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_BulkDiscount;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_BulkDiscount;
using ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_BulkDiscount;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Controllers
{
    [Route("api/price-lists/{priceListId}/bulk-discounts")]
    [ApiController]
    public class BulkDiscountsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BulkDiscountsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseViewModel<BulkDiscountDto>>> Create(
            [FromRoute] Guid priceListId,
            [FromBody] CreateBulkDiscountCommand command)
        {
            command.PriceListId = priceListId;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<List<BulkDiscountDto>>>> GetAll(
            [FromRoute] Guid priceListId)
        {
            var query = new GetBulkDiscountsByPriceListQuery { PriceListId = priceListId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{discountId}")]
        public async Task<ActionResult<ResponseViewModel<BulkDiscountDto>>> GetById(
            [FromRoute] Guid priceListId,
            [FromRoute] Guid discountId)
        {
            var query = new GetBulkDiscountByIdQuery
            {
                Id = discountId,
                PriceListId = priceListId
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{discountId}")]
        public async Task<ActionResult<ResponseViewModel<BulkDiscountDto>>> Update(
            [FromRoute] Guid priceListId,
            [FromRoute] Guid discountId,
            [FromBody] UpdateBulkDiscountCommand command)
        {
            command.Id = discountId;
            command.PriceListId = priceListId;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{discountId}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> Delete(
            [FromRoute] Guid priceListId,
            [FromRoute] Guid discountId)
        {
            var command = new DeleteBulkDiscountCommand
            {
                Id = discountId,
                PriceListId = priceListId
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }

}
