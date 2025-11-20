using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;

namespace ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundInvoce
{
    public class CreateRefundFromPOCommand : IRequest<ResponseViewModel<RefundDto>>
    {
        public Guid PurchaseOrderId { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public bool CreateDebitNote { get; set; } = true;
        public List<CreateRefundLineItemDto> LineItems { get; set; } = new();
    }
}
