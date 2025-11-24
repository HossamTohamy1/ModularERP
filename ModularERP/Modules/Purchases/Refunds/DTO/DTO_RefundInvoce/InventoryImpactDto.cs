namespace ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce
{
    public class InventoryImpactDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSKU { get; set; }
        public Guid WarehouseId { get; set; }
        public decimal QuantityReturned { get; set; }
        public decimal NewStockLevel { get; set; }
    }
}
