using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POItme;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_POItme
{
    public class GetPOLineItemByIdQuery : IRequest<ResponseViewModel<POLineItemResponseDto>>
    {
        public Guid PurchaseOrderId { get; set; }
        public Guid LineItemId { get; set; }
    }
}
