using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder
{
    public class DeletePurchaseOrderCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }
    }

}
