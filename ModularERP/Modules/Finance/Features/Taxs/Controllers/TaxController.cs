using MediatR;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Taxs.Commands;
using ModularERP.Modules.Finance.Features.Taxs.DTO;
using ModularERP.Modules.Finance.Features.Taxs.Queries;
using ModularERP.Common.Enum.Finance_Enum;

namespace ModularERP.Modules.Finance.Features.Taxs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaxController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseViewModel<TaxResponseDto>>> Create([FromBody] CreateTaxDto createTaxDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (createTaxDto == null)
                {
                    var errorResponse = ResponseViewModel<TaxResponseDto>.Error(
                        "Tax data is required",
                        FinanceErrorCode.InvalidData);
                    return BadRequest(errorResponse);
                }

                var command = new CreateTaxCommand(createTaxDto);
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                    return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                var errorResponse = ResponseViewModel<TaxResponseDto>.Error(
                    $"An unexpected error occurred: {ex.Message}",
                    FinanceErrorCode.InternalServerError);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ResponseViewModel<TaxResponseDto>>> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    var errorResponse = ResponseViewModel<TaxResponseDto>.Error(
                        "Invalid tax ID",
                        FinanceErrorCode.InvalidData);
                    return BadRequest(errorResponse);
                }

                var query = new GetTaxByIdQuery(id);
                var result = await _mediator.Send(query);

                if (result.IsSuccess)
                    return Ok(result);
                else if (result.FinanceErrorCode == FinanceErrorCode.NotFound)
                    return NotFound(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                var errorResponse = ResponseViewModel<TaxResponseDto>.Error(
                    $"An unexpected error occurred: {ex.Message}",
                    FinanceErrorCode.InternalServerError);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<IEnumerable<TaxListDto>>>> GetAll()
        {
            try
            {
                var query = new GetAllTaxesQuery();
                var result = await _mediator.Send(query);

                if (result.IsSuccess)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                var errorResponse = ResponseViewModel<IEnumerable<TaxListDto>>.Error(
                    $"An unexpected error occurred: {ex.Message}",
                    FinanceErrorCode.InternalServerError);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<ResponseViewModel<IEnumerable<TaxListDto>>>> GetActive()
        {
            try
            {
                var query = new GetActiveTaxesQuery();
                var result = await _mediator.Send(query);

                if (result.IsSuccess)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                var errorResponse = ResponseViewModel<IEnumerable<TaxListDto>>.Error(
                    $"An unexpected error occurred: {ex.Message}",
                    FinanceErrorCode.InternalServerError);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<ResponseViewModel<IEnumerable<TaxListDto>>>> Search([FromQuery] string searchTerm)
        {
            try
            {
                var query = new SearchTaxesQuery(searchTerm);
                var result = await _mediator.Send(query);

                if (result.IsSuccess)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                var errorResponse = ResponseViewModel<IEnumerable<TaxListDto>>.Error(
                    $"An unexpected error occurred: {ex.Message}",
                    FinanceErrorCode.InternalServerError);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseViewModel<bool>>> Update([FromBody] UpdateTaxDto updateTaxDto)
        {
            try
            {
                if (updateTaxDto == null)
                {
                    var errorResponse = ResponseViewModel<bool>.Error(
                        "Tax data is required",
                        FinanceErrorCode.InvalidData);
                    return BadRequest(errorResponse);
                }

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var command = new UpdateTaxCommand(updateTaxDto);
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                    return Ok(result);
                else if (result.FinanceErrorCode == FinanceErrorCode.NotFound)
                    return NotFound(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                var errorResponse = ResponseViewModel<bool>.Error(
                    $"An unexpected error occurred: {ex.Message}",
                    FinanceErrorCode.InternalServerError);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPatch("{id:guid}/toggle-status")]
        public async Task<ActionResult<ResponseViewModel<bool>>> ToggleActiveStatus(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    var errorResponse = ResponseViewModel<bool>.Error(
                        "Invalid tax ID",
                        FinanceErrorCode.InvalidData);
                    return BadRequest(errorResponse);
                }

                var command = new ToggleTaxStatusCommand(id);
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                    return Ok(result);
                else if (result.FinanceErrorCode == FinanceErrorCode.NotFound)
                    return NotFound(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                var errorResponse = ResponseViewModel<bool>.Error(
                    $"An unexpected error occurred: {ex.Message}",
                    FinanceErrorCode.InternalServerError);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> Delete(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    var errorResponse = ResponseViewModel<bool>.Error(
                        "Invalid tax ID",
                        FinanceErrorCode.InvalidData);
                    return BadRequest(errorResponse);
                }

                var command = new DeleteTaxCommand(id);
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                    return Ok(result);
                else if (result.FinanceErrorCode == FinanceErrorCode.NotFound)
                    return NotFound(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                var errorResponse = ResponseViewModel<bool>.Error(
                    $"An unexpected error occurred: {ex.Message}",
                    FinanceErrorCode.InternalServerError);
                return StatusCode(500, errorResponse);
            }
        }
    }
}
