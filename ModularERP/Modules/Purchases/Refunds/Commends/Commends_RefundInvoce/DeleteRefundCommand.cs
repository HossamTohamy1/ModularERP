using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundInvoce
{
    public class DeleteRefundCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }
    }
}