using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder
{
    public class ClosePurchaseOrderCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }
        public Guid ClosedBy { get; set; }
    }
}
