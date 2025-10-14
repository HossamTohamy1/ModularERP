using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;
using ModularERP.Modules.Inventory.Features.Requisitions.Handlers.Handlers_Requisition;
using ModularERP.Modules.Inventory.Features.Requisitions.Qeuries.Qeuier_Requisition;
using System.Text.Json;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequisitionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RequisitionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a new requisition with file attachments
        /// </summary>
        /// <param name="command">Requisition creation details</param>
        /// <returns>Created requisition</returns>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ResponseViewModel<RequisitionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<RequisitionResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<RequisitionResponseDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseViewModel<RequisitionResponseDto>>> CreateRequisition(
            [FromForm] CreateRequisitionCommand command)
        {
            // تحويل ItemsJson إلى List
            if (!string.IsNullOrEmpty(command.ItemsJson))
            {
                try
                {
                    command.Items = JsonSerializer.Deserialize<List<CreateRequisitionItemDto>>(
                        command.ItemsJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    ) ?? new List<CreateRequisitionItemDto>();
                }
                catch (JsonException ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }



        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<PaginatedResponseViewModel<RequisitionListDto>>>> GetRequisitions(
                    [FromQuery] GetRequisitionsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseViewModel<RequisitionResponseDto>>> GetRequisitionById(
            Guid id,
            [FromQuery] Guid companyId)
        {
            var query = new GetRequisitionByIdQuery
            {
                Id = id,
                CompanyId = companyId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseViewModel<RequisitionResponseDto>>> UpdateRequisition(
            Guid id,
            [FromForm] UpdateRequisitionCommand command)
        {
            command.Id = id;

            // Deserialize ItemsJson to Items list
            if (!string.IsNullOrEmpty(command.ItemsJson))
            {
                command.Items = JsonSerializer.Deserialize<List<CreateRequisitionItemDto>>(command.ItemsJson)
                    ?? new List<CreateRequisitionItemDto>();
            }

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> DeleteRequisition(
            Guid id,
            [FromQuery] Guid companyId)
        {
            var command = new DeleteRequisitionCommand
            {
                Id = id,
                CompanyId = companyId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("by-status/{status}")]
        public async Task<ActionResult<ResponseViewModel<List<RequisitionListDto>>>> GetRequisitionsByStatus(
            int status,
            [FromQuery] Guid companyId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetRequisitionsByStatusQuery
            {
                CompanyId = companyId,
                Status = (ModularERP.Common.Enum.Inventory_Enum.RequisitionStatus)status,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("by-warehouse/{warehouseId}")]
        public async Task<ActionResult<ResponseViewModel<List<RequisitionListDto>>>> GetRequisitionsByWarehouse(
            Guid warehouseId,
            [FromQuery] Guid companyId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetRequisitionsByWarehouseQuery
            {
                CompanyId = companyId,
                WarehouseId = warehouseId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("pending-approval")]
        public async Task<ActionResult<ResponseViewModel<List<RequisitionListDto>>>> GetPendingApprovalRequisitions(
            [FromQuery] Guid companyId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetPendingApprovalRequisitionsQuery
            {
                CompanyId = companyId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
    
