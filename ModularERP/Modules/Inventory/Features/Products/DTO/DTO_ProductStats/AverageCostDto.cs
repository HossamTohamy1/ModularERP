namespace ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ProductStats
{
    public class AverageCostDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal AvgUnitCost { get; set; }
        public DateTime LastCalculatedAt { get; set; }
    }
}
