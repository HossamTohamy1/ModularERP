namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockSnapshot
{
    public class SnapshotListDto
    {
        public Guid StocktakingId { get; set; }
        public string StocktakingNumber { get; set; } = string.Empty;
        public DateTime SnapshotDate { get; set; }
        public int TotalProducts { get; set; }
        public List<StockSnapshotDto> Snapshots { get; set; } = new();
    }
}
