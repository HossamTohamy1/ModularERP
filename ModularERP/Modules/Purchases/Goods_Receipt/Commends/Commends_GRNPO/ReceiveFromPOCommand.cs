using MediatR;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRNPO
{
    public class ReceiveFromPOCommand : IRequest<GRNResponseDto>
    {
        public Guid PurchaseOrderId { get; set; }
        public Guid WarehouseId { get; set; }
        public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;
        public string? ReceivedBy { get; set; }
        public string? Notes { get; set; }
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        public List<CreateGRNLineItemDto> LineItems { get; set; } = new();
    }
}