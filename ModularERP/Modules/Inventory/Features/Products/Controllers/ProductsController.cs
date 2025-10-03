using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.Commends;
using ModularERP.Modules.Inventory.Features.Products.Commends.Commends_Product;
using ModularERP.Modules.Inventory.Features.Products.DTO;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_Product;
using ModularERP.Modules.Inventory.Features.Products.Qeuries;
using ModularERP.Modules.Inventory.Features.Products.Qeuries.Qeuries_Product;

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

        /// <summary>
        /// Get paginated list of products with filters and search
        /// </summary>
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

        /// <summary>
        /// Get detailed information for a specific product
        /// </summary>
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

        /// <summary>
        /// Create a new product
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseViewModel<ProductDetailsDto>), StatusCodes.Status200OK)]
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

        /// <summary>
        /// Update an existing product
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<ProductDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(ResponseViewModel<object>.Error(
                    "Product ID in URL does not match ID in request body",
                    Common.Enum.Finance_Enum.FinanceErrorCode.ValidationError));
            }

            _logger.LogInformation("Updating product {ProductId} for company {CompanyId}",
                id, dto.CompanyId);

            var command = new UpdateProductCommand(dto);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Soft delete a product
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteProduct(Guid id, [FromQuery] Guid companyId)
        {
            _logger.LogInformation("Deleting product {ProductId} for company {CompanyId}",
                id, companyId);

            var command = new DeleteProductCommand(id, companyId);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Clone an existing product with a new SKU
        /// </summary>
        [HttpPost("{id}/clone")]
        [ProducesResponseType(typeof(ResponseViewModel<ProductDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CloneProduct(Guid id, [FromQuery] Guid companyId)
        {
            _logger.LogInformation("Cloning product {ProductId} for company {CompanyId}",
                id, companyId);

            var command = new CloneProductCommand(id, companyId);
            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}