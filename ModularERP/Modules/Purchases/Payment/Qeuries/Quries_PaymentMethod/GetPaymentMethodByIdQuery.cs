using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentMethod;

namespace ModularERP.Modules.Purchases.Payment.Qeuries.Quries_PaymentMethod
{
    public class GetPaymentMethodByIdQuery : IRequest<ResponseViewModel<PaymentMethodDto>>
    {
        public Guid Id { get; set; }

        public GetPaymentMethodByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
