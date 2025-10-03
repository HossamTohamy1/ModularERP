namespace ModularERP.Modules.Inventory.Features.Products.DTO.DTO_Product
{
    public class ProductStatsDto
    {
        public decimal OnHandStock { get; set; }
        public decimal TotalSold { get; set; }
        public decimal SoldLast28Days { get; set; }
        public decimal SoldLast7Days { get; set; }
        public decimal AverageUnitCost { get; set; }
        public string StockStatus { get; set; } // InStock,
    }
}
