using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_POStauts
{
    public record GetPOPdfQuery(Guid PurchaseOrderId) : IRequest<ResponseViewModel<byte[]>>;

}
