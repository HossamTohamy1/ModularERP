using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Purchases.Payment.Commends.Commends_PaymentTerm
{
    public class DeletePaymentTermCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }

        public DeletePaymentTermCommand(Guid id)
        {
            Id = id;
        }
    }
}