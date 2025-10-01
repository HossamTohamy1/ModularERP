using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO;

namespace ModularERP.Modules.Inventory.Features.Products.Qeuries
{
    public class GetProductsListQuery : IRequest<ResponseViewModel<PaginatedProductListDto>>
    {
        public ProductListRequestDto Request { get; set; }

        public GetProductsListQuery(ProductListRequestDto request)
        {
            Request = request;
        }
    }
}