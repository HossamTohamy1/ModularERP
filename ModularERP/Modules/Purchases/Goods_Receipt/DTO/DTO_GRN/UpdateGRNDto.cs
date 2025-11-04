namespace ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN
{
    public class UpdateGRNDto
    {
        public Guid Id { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public Guid WarehouseId { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string? ReceivedBy { get; set; }
        public string? Notes { get; set; }
        public List<UpdateGRNLineItemDto> LineItems { get; set; } = new();
    }
}
