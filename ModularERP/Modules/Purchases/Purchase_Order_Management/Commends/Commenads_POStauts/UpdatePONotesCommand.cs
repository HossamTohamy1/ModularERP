using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commenads_POStauts
{
    public record UpdatePONotesCommand(
        Guid PurchaseOrderId,
        string? Notes,
        string? Terms
    ) : IRequest<ResponseViewModel<bool>>;
}
