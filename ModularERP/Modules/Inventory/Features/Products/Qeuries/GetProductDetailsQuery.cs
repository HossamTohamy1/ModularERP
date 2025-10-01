using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO;

namespace ModularERP.Modules.Inventory.Features.Products.Qeuries
{
    public class GetProductDetailsQuery : IRequest<ResponseViewModel<ProductDetailsDto>>
    {
        public Guid ProductId { get; set; }
        public Guid CompanyId { get; set; }

        public GetProductDetailsQuery(Guid productId, Guid companyId)
        {
            ProductId = productId;
            CompanyId = companyId;
        }
    }
}
