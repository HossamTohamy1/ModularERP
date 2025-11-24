using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_SupplierPayments;

namespace ModularERP.Modules.Purchases.Invoicing.Qeuries.Qeuries_SupplierPayments
{
    public record GetSupplierPaymentByIdQuery(Guid Id)
        : IRequest<ResponseViewModel<SupplierPaymentDto>>;
}
