namespace ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN
{
    public class GRNListItemDto
    {
        public Guid Id { get; set; }
        public string GRNNumber { get; set; } = string.Empty;
        public string PurchaseOrderNumber { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public DateTime ReceiptDate { get; set; }
        public string? ReceivedBy { get; set; }
        public int LineItemsCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
