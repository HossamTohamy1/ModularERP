using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_Product;
using ModularERP.Modules.Inventory.Features.Products.DTO;

namespace ModularERP.Modules.Inventory.Features.Products.Commends.Commends_Product
{
    public class UpdateProductCommand : IRequest<ResponseViewModel<ProductDetailsDto>>
    {
        public UpdateProductDto ProductDto { get; set; }

        public UpdateProductCommand(UpdateProductDto productDto)
        {
            ProductDto = productDto;
        }
    }
}
