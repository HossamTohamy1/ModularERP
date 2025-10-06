using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_Product;

namespace ModularERP.Modules.Inventory.Features.Products.DTO
{
    public class ProductDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public string? Description { get; set; }
        public string? PhotoUrl { get; set; }
        public string Status { get; set; }

        // Company & Warehouse
        public Guid CompanyId { get; set; }
        public Guid WarehouseId { get; set; }
        public string? WarehouseName { get; set; }

        // Pricing
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal? MinimumPrice { get; set; }
        public decimal? Discount { get; set; }
        public string? DiscountType { get; set; }
        public decimal? ProfitMargin { get; set; }

        // Inventory
        public bool TrackStock { get; set; }
        public decimal? InitialStock { get; set; }
        public decimal? LowStockThreshold { get; set; }

        // References
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Guid? BrandId { get; set; }
        public string? BrandName { get; set; }
        public Guid? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? Barcode { get; set; }
        public Guid? ItemGroupId { get; set; }
        public string? ItemGroupName { get; set; }

        // Stats
        public ProductStatsDto? Stats { get; set; }

        // Metadata
        public string? InternalNotes { get; set; }
        public string? Tags { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
