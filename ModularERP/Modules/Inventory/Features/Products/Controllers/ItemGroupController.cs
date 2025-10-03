using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.Commends.Commands_ItemGroup;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ItemGroup;
using ModularERP.Modules.Inventory.Features.Products.Qeuries.Queries_ItemGroup;

namespace ModularERP.Modules.Inventory.Features.Products.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemGroupController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ItemGroupController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<List<ItemGroupDto>>>> GetAll()
        {
            var result = await _mediator.Send(new GetAllItemGroupsQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseViewModel<ItemGroupDetailDto>>> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetItemGroupByIdQuery { Id = id });
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ResponseViewModel<ItemGroupDto>>> Create([FromBody] CreateItemGroupCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseViewModel<ItemGroupDto>>> Update(Guid id, [FromBody] UpdateItemGroupCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteItemGroupCommand { Id = id });
            return Ok(result);
        }

        [HttpPost("{id}/items")]
        public async Task<ActionResult<ResponseViewModel<ItemGroupItemDto>>> AddItem(Guid id, [FromBody] AddItemToGroupCommand command)
        {
            command.GroupId = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}/items/{itemId}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> RemoveItem(Guid id, Guid itemId)
        {
            var result = await _mediator.Send(new RemoveItemFromGroupCommand { GroupId = id, ItemId = itemId });
            return Ok(result);
        }
    }
}
