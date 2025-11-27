using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentTerm;

namespace ModularERP.Modules.Purchases.Payment.Commends.Commends_PaymentTerm
{
    public class UpdatePaymentTermCommand : IRequest<ResponseViewModel<PaymentTermResponseDto>>
    {
        public Guid Id { get; set; }
        public UpdatePaymentTermDto Dto { get; set; }

        public UpdatePaymentTermCommand(Guid id, UpdatePaymentTermDto dto)
        {
            Id = id;
            Dto = dto;
        }
    }
}
