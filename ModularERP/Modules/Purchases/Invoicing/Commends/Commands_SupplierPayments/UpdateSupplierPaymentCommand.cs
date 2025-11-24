using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_SupplierPayments;

namespace ModularERP.Modules.Purchases.Invoicing.Commends.Commands_SupplierPayments
{
    public record UpdateSupplierPaymentCommand(UpdateSupplierPaymentDto Dto)
        : IRequest<ResponseViewModel<SupplierPaymentDto>>;
}
