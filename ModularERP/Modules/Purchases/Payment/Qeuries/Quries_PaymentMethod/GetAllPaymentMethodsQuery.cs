using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentMethod;

namespace ModularERP.Modules.Purchases.Payment.Qeuries.Quries_PaymentMethod
{
    public class GetAllPaymentMethodsQuery : IRequest<ResponseViewModel<List<PaymentMethodDto>>>
    {
        public bool? IsActive { get; set; }
    }
}