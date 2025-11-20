using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;

namespace ModularERP.Modules.Purchases.Refunds.Qeuries.Qeuries_RefundInvoce
{
    public class GetRefundByIdQuery : IRequest<ResponseViewModel<RefundDto>>
    {
        public Guid Id { get; set; }
    }
}
