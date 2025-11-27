using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentMethod;

namespace ModularERP.Modules.Purchases.Payment.Commends.Commends_PaymentMethod
{
    public class UpdatePaymentMethodCommand : IRequest<ResponseViewModel<PaymentMethodDto>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string? Description { get; set; }
        public bool RequiresReference { get; set; }
        public bool IsActive { get; set; }
    }
}
