namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder
{
    public class PODiscountDto
    {
        public Guid Id { get; set; }
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? Description { get; set; }
    }
}
