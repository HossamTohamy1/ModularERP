namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockSnapshot
{
    public class RefreshSnapshotDto
    {
        public Guid StocktakingId { get; set; }
        public int ProductsRefreshed { get; set; }
        public DateTime RefreshedAt { get; set; }
        public List<StockSnapshotDto> UpdatedSnapshots { get; set; } = new();
    }
}
