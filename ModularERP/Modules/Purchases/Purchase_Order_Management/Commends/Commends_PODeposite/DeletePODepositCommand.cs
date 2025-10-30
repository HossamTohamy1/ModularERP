using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PODeposite
{
    public class DeletePODepositCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }
        public Guid PurchaseOrderId { get; set; }
    }
}
