using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;

namespace ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundInvoce
{
    public class CreateRefundCommand : IRequest<ResponseViewModel<RefundDto>>
    {
        public Guid PurchaseOrderId { get; set; }
        public Guid SupplierId { get; set; }
        public DateTime RefundDate { get; set; } = DateTime.UtcNow;
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public bool CreateDebitNote { get; set; } = true;
        public List<CreateRefundLineItemDto> LineItems { get; set; } = new();
    }
}
