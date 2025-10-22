namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockSnapshot
{
    public class StockSnapshotDto
    {
        public Guid SnapshotId { get; set; }
        public Guid StocktakingId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public decimal QtyAtStart { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
