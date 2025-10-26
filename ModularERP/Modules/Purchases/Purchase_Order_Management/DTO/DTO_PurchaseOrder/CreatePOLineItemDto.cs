namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder
{
    public class CreatePOLineItemDto
    {
        public Guid? ProductId { get; set; }
        public Guid? ServiceId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public Guid? TaxProfileId { get; set; }
    }
}
