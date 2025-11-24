using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_PaymentApplication;

namespace ModularERP.Modules.Purchases.Invoicing.Commends.Commands_PaymentApplication
{
    public record ApplyPaymentCommand(Guid PaymentId, ApplyPaymentDto Dto)
         : IRequest<ResponseViewModel<PaymentApplicationSummaryDto>>;
}
