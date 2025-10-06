using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Command_Custom;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Custom;
using ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handler_CustomField;
using ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Queries_CutomField;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomFieldController : ControllerBase
    {
        private readonly CreateCustomFieldHandler _createHandler;
        private readonly UpdateCustomFieldHandler _updateHandler;
        private readonly DeleteCustomFieldHandler _deleteHandler;
        private readonly GetAllCustomFieldsHandler _getAllHandler;
        private readonly GetCustomFieldByIdHandler _getByIdHandler;
        private readonly GetCustomFieldsByEntityHandler _getByEntityHandler;

        public CustomFieldController(
            CreateCustomFieldHandler createHandler,
            UpdateCustomFieldHandler updateHandler,
            DeleteCustomFieldHandler deleteHandler,
            GetAllCustomFieldsHandler getAllHandler,
            GetCustomFieldByIdHandler getByIdHandler,
            GetCustomFieldsByEntityHandler getByEntityHandler)
        {
            _createHandler = createHandler;
            _updateHandler = updateHandler;
            _deleteHandler = deleteHandler;
            _getAllHandler = getAllHandler;
            _getByIdHandler = getByIdHandler;
            _getByEntityHandler = getByEntityHandler;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseViewModel<CustomFieldResponseDto>>> Create([FromBody] CreateCustomFieldCommand command)
        {
            var result = await _createHandler.Handle(command);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<List<CustomFieldResponseDto>>>> GetAll()
        {
            var result = await _getAllHandler.Handle(new GetAllCustomFieldsQuery());
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ResponseViewModel<CustomFieldResponseDto>>> GetById(Guid id)
        {
            var result = await _getByIdHandler.Handle(new GetCustomFieldByIdQuery { Id = id });
            return Ok(result);
        }

        [HttpGet("by-entity/{entityType}")]
        public async Task<ActionResult<ResponseViewModel<List<CustomFieldResponseDto>>>> GetByEntity(string entityType)
        {
            var result = await _getByEntityHandler.Handle(new GetCustomFieldsByEntityQuery { EntityType = entityType });
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ResponseViewModel<CustomFieldResponseDto>>> Update(Guid id, [FromBody] UpdateCustomFieldCommand command)
        {
            command.Id = id;
            var result = await _updateHandler.Handle(command);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> Delete(Guid id)
        {
            var result = await _deleteHandler.Handle(new DeleteCustomFieldCommand { Id = id });
            return Ok(result);
        }
    }
}

