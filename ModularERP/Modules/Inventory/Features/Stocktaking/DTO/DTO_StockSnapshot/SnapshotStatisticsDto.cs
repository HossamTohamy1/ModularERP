namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockSnapshot
{
    public class SnapshotStatisticsDto
    {
        public Guid StocktakingId { get; set; }
        public string StocktakingNumber { get; set; } = string.Empty;
        public DateTime SnapshotDate { get; set; }
        public int TotalProducts { get; set; }
        public int ProductsWithDrift { get; set; }
        public int ProductsExceedingThreshold { get; set; }
        public decimal TotalSnapshotValue { get; set; }
        public decimal TotalCurrentValue { get; set; }
        public decimal TotalDriftValue { get; set; }
        public decimal AverageDriftPercentage { get; set; }
        public decimal MaxDriftPercentage { get; set; }
        public string ProductWithMaxDrift { get; set; } = string.Empty;
    }
}
