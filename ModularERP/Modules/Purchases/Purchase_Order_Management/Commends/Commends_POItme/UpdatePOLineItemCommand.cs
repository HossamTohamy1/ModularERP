using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POItme;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_POItme
{
    public class UpdatePOLineItemCommand : IRequest<ResponseViewModel<POLineItemResponseDto>>
    {
        public Guid PurchaseOrderId { get; set; }
        public Guid LineItemId { get; set; }
        public UpdatePOLineItemDto LineItem { get; set; } = null!;
    }
}
