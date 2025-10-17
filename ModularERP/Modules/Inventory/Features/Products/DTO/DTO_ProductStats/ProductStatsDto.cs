namespace ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ProductStats
{
    public class ProductStatsDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSku { get; set; }
        public decimal TotalSold { get; set; }
        public decimal SoldLast28Days { get; set; }
        public decimal SoldLast7Days { get; set; }
        public decimal OnHandStock { get; set; }
        public decimal AvgUnitCost { get; set; }
        public DateTime LastCalculatedAt { get; set; }
        public bool IsActive { get; set; }
    }

}
