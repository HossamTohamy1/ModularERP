using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentTerm;

namespace ModularERP.Modules.Purchases.Payment.Qeuries.Queries_PaymentTerm
{
    public class GetAllPaymentTermsQuery : IRequest<ResponseViewModel<List<PaymentTermResponseDto>>>
    {
        public bool? IsActive { get; set; }

        public GetAllPaymentTermsQuery(bool? isActive = null)
        {
            IsActive = isActive;
        }
    }
}
