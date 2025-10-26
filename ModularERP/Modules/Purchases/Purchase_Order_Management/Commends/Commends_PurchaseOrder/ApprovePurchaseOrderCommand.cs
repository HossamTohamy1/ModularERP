using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder
{
    public class ApprovePurchaseOrderCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }
        public Guid ApprovedBy { get; set; }
        public string? ApprovalNotes { get; set; }
    }
}
