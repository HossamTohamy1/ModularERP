using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundItem;

namespace ModularERP.Modules.Purchases.Refunds.Qeuries.Qeuries_RefundItem
{
    public record GetRefundItemsQuery : IRequest<ResponseViewModel<List<RefundItemListDto>>>
    {
        public Guid RefundId { get; init; }
    }
}
