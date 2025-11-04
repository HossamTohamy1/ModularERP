namespace ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN
{
    public class GRNLineItemResponseDto
    {
        public Guid Id { get; set; }
        public Guid POLineItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;  
        public decimal ReceivedQuantity { get; set; }
        public string? Notes { get; set; }
    }
}