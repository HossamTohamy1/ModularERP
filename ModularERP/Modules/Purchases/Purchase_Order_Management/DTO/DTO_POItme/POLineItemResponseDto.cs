namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POItme
{
    public class POLineItemResponseDto
    {
        public Guid Id { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public Guid? ProductId { get; set; }
        public string? ProductName { get; set; }
        public Guid? ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public Guid? TaxProfileId { get; set; }
        public string? TaxProfileName { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal InvoicedQuantity { get; set; }
        public decimal ReturnedQuantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
