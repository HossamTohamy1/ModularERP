using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundItem
{
    public record DeleteRefundItemCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid RefundId { get; init; }
        public Guid ItemId { get; init; }
    }
}

