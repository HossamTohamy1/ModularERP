namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO
{
    public class StockTransactionSummaryDto
    {
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public Guid WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal NetMovement { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal? AverageUnitCost { get; set; }
        public int TransactionCount { get; set; }
    }
}
