using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Category;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Category;
using ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries;
using ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Qeuries_Category;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

  
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<CategoryListDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? parentCategoryId = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetAllCategoriesQuery(searchTerm, parentCategoryId, pageNumber, pageSize);
            var result = await _mediator.Send(query);
            return Ok(result);
        }


        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetCategoryByIdQuery(id);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

  
        [HttpGet("hierarchy")]
        [ProducesResponseType(typeof(List<CategoryHierarchyDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHierarchy()
        {
            var query = new GetCategoryHierarchyQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }


        [HttpPost]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromForm] CreateCategoryDto dto)
        {
            var command = new CreateCategoryCommand(dto);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Data.Id },
                    result);
            }

            return BadRequest(result);
        }

 
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateCategoryDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID mismatch");
            }

            var command = new UpdateCategoryCommand(dto);
            var result = await _mediator.Send(command);
            return Ok(result);
        }


        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteCategoryCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

 
        [HttpDelete("{categoryId:guid}/attachments/{attachmentId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAttachment(Guid categoryId, Guid attachmentId)
        {
            var command = new DeleteCategoryAttachmentCommand(categoryId, attachmentId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}

