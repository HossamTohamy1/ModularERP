using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_POItme
{
    public class DeletePOLineItemCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid PurchaseOrderId { get; set; }
        public Guid LineItemId { get; set; }
    }
}
