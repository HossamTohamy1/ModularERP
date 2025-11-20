using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundItem;

namespace ModularERP.Modules.Purchases.Refunds.Qeuries.Qeuries_RefundItem
{
    public record GetRefundItemByIdQuery : IRequest<ResponseViewModel<RefundItemDto>>
    {
        public Guid RefundId { get; init; }
        public Guid ItemId { get; init; }
    }
}
