using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.Commends;
using ModularERP.Modules.Inventory.Features.Products.DTO;
using ModularERP.Modules.Inventory.Features.Products.Qeuries;

namespace ModularERP.Modules.Inventory.Features.Products.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ResponseViewModel<PaginatedProductListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProducts([FromQuery] ProductListRequestDto request)
        {
            _logger.LogInformation("Getting products list for company {CompanyId}", request.CompanyId);

            var query = new GetProductsListQuery(request);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<ProductDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetProductById(Guid id, [FromQuery] Guid companyId)
        {
            _logger.LogInformation("Getting product details for ID {ProductId}", id);

            var query = new GetProductDetailsQuery(id, companyId);
            var result = await _mediator.Send(query);

            return Ok(result);
        }


        [HttpPost]
        [ProducesResponseType(typeof(ResponseViewModel<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            _logger.LogInformation("Creating new product with SKU {SKU} for company {CompanyId}",
                dto.SKU, dto.CompanyId);

            var command = new CreateProductCommand(dto);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
