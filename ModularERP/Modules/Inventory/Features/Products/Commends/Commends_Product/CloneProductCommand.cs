using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO;

namespace ModularERP.Modules.Inventory.Features.Products.Commends.Commends_Product
{
    public class CloneProductCommand : IRequest<ResponseViewModel<ProductDetailsDto>>
    {
        public Guid ProductId { get; set; }
        public Guid CompanyId { get; set; }

        public CloneProductCommand(Guid productId, Guid companyId)
        {
            ProductId = productId;
            CompanyId = companyId;
        }
    }
}
