using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO;

namespace ModularERP.Modules.Inventory.Features.Products.Commends
{
    public class CreateProductCommand : IRequest<ResponseViewModel<Guid>>
    {
        public CreateProductDto ProductDto { get; set; }

        public CreateProductCommand(CreateProductDto productDto)
        {
            ProductDto = productDto;
        }
    }
}
