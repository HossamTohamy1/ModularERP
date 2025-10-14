using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;
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
    
        ///// <summary>
        ///// Get all requisitions with filtering and pagination
        ///// </summary>
        ///// <param name="query">Query parameters for filtering</param>
        ///// <returns>List of requisitions</returns>
        //[HttpGet]
        //[ProducesResponseType(typeof(ResponseViewModel<List<RequisitionListDto>>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ResponseViewModel<List<RequisitionListDto>>), StatusCodes.Status400BadRequest)]
        //public async Task<ActionResult<ResponseViewModel<List<RequisitionListDto>>>> GetRequisitions(
        //    [FromQuery] GetRequisitionsQuery query)
        //{
        //    var result = await _mediator.Send(query);

        //    if (!result.IsSuccess)
        //    {
        //        return BadRequest(result);
        //    }

        //    return Ok(result);
        //}
    }
}