namespace ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN
{
    public class GRNLineItemResponseDto
    {
        public Guid Id { get; set; }
        public Guid GRNId { get; set; }
        public Guid POLineItemId { get; set; }
        public Guid? ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;

        // ✨ NEW: Quantity Tracking
        public decimal OrderedQuantity { get; set; }
        public decimal PreviouslyReceivedQuantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public bool IsFullyReceived { get; set; }

        public string? UnitOfMeasure { get; set; }
        public string? Notes { get; set; }
    }
}