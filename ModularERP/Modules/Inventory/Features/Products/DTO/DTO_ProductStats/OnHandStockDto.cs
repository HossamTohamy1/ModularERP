namespace ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ProductStats
{
    public class OnHandStockDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSku { get; set; }
        public decimal OnHandStock { get; set; }
        public string StockStatus { get; set; }
        public DateTime AsOfDate { get; set; }
    }
}
