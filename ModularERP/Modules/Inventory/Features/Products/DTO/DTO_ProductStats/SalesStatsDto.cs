namespace ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ProductStats
{
    public class SalesStatsDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal TotalSold { get; set; }
        public decimal SoldLast28Days { get; set; }
        public decimal SoldLast7Days { get; set; }
        public decimal Last7DaysGrowth { get; set; }
        public decimal Last28DaysGrowth { get; set; }
    }
}
