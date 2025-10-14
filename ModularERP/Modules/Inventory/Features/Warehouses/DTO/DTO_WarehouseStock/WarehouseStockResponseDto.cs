namespace ModularERP.Modules.Inventory.Features.Warehouses.DTO.DTO_WarehouseStock
{
    public class WarehouseStockResponseDto
    {
        public Guid Id { get; set; }
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal? ReservedQuantity { get; set; }
        public decimal AvailableQuantity { get; set; }
        public decimal? AverageUnitCost { get; set; }
        public decimal? TotalValue { get; set; }
        public decimal? MinStockLevel { get; set; }
        public decimal? MaxStockLevel { get; set; }
        public decimal? ReorderPoint { get; set; }
        public DateTime? LastStockInDate { get; set; }
        public DateTime? LastStockOutDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
