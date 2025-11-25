namespace ModularERP.Modules.Purchases.KPIs.DTO
{
    public class PurchaseVolumeDto
    {
        public decimal TotalPurchaseAmount { get; set; }
        public int TotalPurchaseOrders { get; set; }
        public decimal AveragePurchaseValue { get; set; }
        public decimal MonthlyGrowthPercentage { get; set; }
        public List<MonthlyVolumeDto> MonthlyBreakdown { get; set; } = new();
    }
    public class MonthlyVolumeDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int OrderCount { get; set; }
    }
}
