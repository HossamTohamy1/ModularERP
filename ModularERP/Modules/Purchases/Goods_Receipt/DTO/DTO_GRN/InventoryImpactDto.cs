namespace ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN
{
    public class InventoryImpactDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public decimal QuantityAdded { get; set; }
        public decimal PreviousStock { get; set; }
        public decimal NewStock { get; set; }
    }
}
