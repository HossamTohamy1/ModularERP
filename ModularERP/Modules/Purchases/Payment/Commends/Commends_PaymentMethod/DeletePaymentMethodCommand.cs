using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Purchases.Payment.Commends.Commends_PaymentMethod
{
    public class DeletePaymentMethodCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }

        public DeletePaymentMethodCommand(Guid id)
        {
            Id = id;
        }
    }
}

