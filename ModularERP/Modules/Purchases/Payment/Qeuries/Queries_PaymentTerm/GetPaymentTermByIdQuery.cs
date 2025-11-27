using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentTerm;

namespace ModularERP.Modules.Purchases.Payment.Qeuries.Queries_PaymentTerm
{
    public class GetPaymentTermByIdQuery : IRequest<ResponseViewModel<PaymentTermResponseDto>>
    {
        public Guid Id { get; set; }

        public GetPaymentTermByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
