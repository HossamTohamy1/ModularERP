using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundItem;

namespace ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundItem
{
    public record UpdateRefundItemCommand : IRequest<ResponseViewModel<RefundItemDto>>
    {
        public Guid RefundId { get; init; }
        public Guid ItemId { get; init; }
        public decimal ReturnQuantity { get; init; }
        public decimal UnitPrice { get; init; }
    }
}
