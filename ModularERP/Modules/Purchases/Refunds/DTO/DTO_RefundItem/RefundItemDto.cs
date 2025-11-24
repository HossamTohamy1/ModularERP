namespace ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundItem
{
    public class RefundItemDto
    {
        public Guid Id { get; set; }
        public Guid RefundId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductSKU { get; set; }
        public Guid GRNLineItemId { get; set; }
        public decimal ReturnQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public DateTime CreatedAt { get; set; }

        // Projected properties from GRNLineItem
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
    }
}
