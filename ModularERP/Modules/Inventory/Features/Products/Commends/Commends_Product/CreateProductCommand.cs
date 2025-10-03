using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_Product;

namespace ModularERP.Modules.Inventory.Features.Products.Commends.Commends_Product
{
    public class CreateProductCommand : IRequest<ResponseViewModel<ProductDetailsDto>>
    {
        public CreateProductDto ProductDto { get; }

        public CreateProductCommand(CreateProductDto productDto)
        {
            ProductDto = productDto;
        }
    }
}
