using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder
{
    public class RejectPurchaseOrderCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }
        public Guid RejectedBy { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
    }
}
