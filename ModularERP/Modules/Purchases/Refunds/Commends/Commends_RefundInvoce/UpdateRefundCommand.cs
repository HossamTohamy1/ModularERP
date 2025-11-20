using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;

namespace ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundInvoce
{
    public class UpdateRefundCommand : IRequest<ResponseViewModel<RefundDto>>
    {
        public Guid Id { get; set; }
        public DateTime RefundDate { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public List<CreateRefundLineItemDto> LineItems { get; set; } = new();
    }
}
