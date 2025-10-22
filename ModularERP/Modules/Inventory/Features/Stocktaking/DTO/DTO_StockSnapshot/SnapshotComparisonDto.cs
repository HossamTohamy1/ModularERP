namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockSnapshot
{
    public class SnapshotComparisonDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public decimal SnapshotQty { get; set; }
        public decimal CurrentQty { get; set; }
        public decimal Drift { get; set; }
        public decimal DriftPercentage { get; set; }
        public bool ExceedsThreshold { get; set; }
    }
}
