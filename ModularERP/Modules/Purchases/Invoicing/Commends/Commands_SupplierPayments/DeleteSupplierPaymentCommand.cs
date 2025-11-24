using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Purchases.Invoicing.Commends.Commands_SupplierPayments
{
    public record DeleteSupplierPaymentCommand(Guid Id)
        : IRequest<ResponseViewModel<bool>>;
}
