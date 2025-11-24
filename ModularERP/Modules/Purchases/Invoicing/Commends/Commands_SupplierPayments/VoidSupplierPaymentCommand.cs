using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Purchases.Invoicing.Commends.Commands_SupplierPayments
{
    public record VoidSupplierPaymentCommand(Guid Id, string VoidReason, Guid VoidedBy)
        : IRequest<ResponseViewModel<bool>>;
}
