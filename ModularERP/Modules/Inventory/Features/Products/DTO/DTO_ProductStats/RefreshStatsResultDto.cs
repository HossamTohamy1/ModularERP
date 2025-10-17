namespace ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ProductStats
{
    public class RefreshStatsResultDto
    {
        public Guid ProductId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime RefreshedAt { get; set; }
        public ProductStatsDto UpdatedStats { get; set; }
    }
}
