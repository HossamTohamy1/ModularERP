using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POStatus;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_POStauts
{
    public record GetPOTimelineQuery(Guid PurchaseOrderId) : IRequest<ResponseViewModel<POTimelineDto>>;

}
