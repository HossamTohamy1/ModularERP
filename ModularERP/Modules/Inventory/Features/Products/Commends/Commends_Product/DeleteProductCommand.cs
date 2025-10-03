using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.Products.Commends.Commends_Product
{
    public class DeleteProductCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid ProductId { get; set; }
        public Guid CompanyId { get; set; }

        public DeleteProductCommand(Guid productId, Guid companyId)
        {
            ProductId = productId;
            CompanyId = companyId;
        }
    }
}
