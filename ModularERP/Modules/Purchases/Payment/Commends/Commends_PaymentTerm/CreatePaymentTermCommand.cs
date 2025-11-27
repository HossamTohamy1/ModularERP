using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentTerm;

namespace ModularERP.Modules.Purchases.Payment.Commends.Commends_PaymentTerm
{
    public class CreatePaymentTermCommand : IRequest<ResponseViewModel<PaymentTermResponseDto>>
    {
        public CreatePaymentTermDto Dto { get; set; }

        public CreatePaymentTermCommand(CreatePaymentTermDto dto)
        {
            Dto = dto;
        }
    }
}