using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundItem;

namespace ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundItem
{
    public record PostRefundCommand : IRequest<ResponseViewModel<PostRefundResponseDto>>
    {
        public Guid RefundId { get; init; }
        public string? Notes { get; init; }
    }
}
