using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Brand;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Brand;
using ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Qeuires_Brand;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BrandsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<List<BrandDto>>>> GetAll()
        {
            var result = await _mediator.Send(new GetAllBrandsQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseViewModel<BrandDto>>> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetBrandByIdQuery(id));
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ResponseViewModel<BrandDto>>> Create([FromBody] CreateBrandDto dto)
        {
            var result = await _mediator.Send(new CreateBrandCommand(dto));
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseViewModel<BrandDto>>> Update(Guid id, [FromBody] UpdateBrandDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(ResponseViewModel<BrandDto>.Error(
                    "Brand ID in URL does not match ID in request body",
                    Common.Enum.Finance_Enum.FinanceErrorCode.ValidationError));
            }

            var result = await _mediator.Send(new UpdateBrandCommand(dto));
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteBrandCommand(id));
            return Ok(result);
        }
    }
}

