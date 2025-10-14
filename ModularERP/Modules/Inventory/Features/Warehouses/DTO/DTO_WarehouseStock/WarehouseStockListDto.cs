namespace ModularERP.Modules.Inventory.Features.Warehouses.DTO.DTO_WarehouseStock
{
    public class WarehouseStockListDto
    {
        public Guid Id { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal AvailableQuantity { get; set; }
        public decimal? AverageUnitCost { get; set; }
        public decimal? TotalValue { get; set; }
        public bool IsLowStock { get; set; }
    }
}
