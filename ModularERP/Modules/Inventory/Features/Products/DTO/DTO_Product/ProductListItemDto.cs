namespace ModularERP.Modules.Inventory.Features.Products.DTO.DTO_Product
{
    public class ProductListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public string? PhotoUrl { get; set; }
        public string CategoryName { get; set; }
        public string? BrandName { get; set; }
        public string? WarehouseName { get; set; }  // NEW
        public decimal SellingPrice { get; set; }
        public decimal? OnHandStock { get; set; }
        public string Status { get; set; }
        public bool TrackStock { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
