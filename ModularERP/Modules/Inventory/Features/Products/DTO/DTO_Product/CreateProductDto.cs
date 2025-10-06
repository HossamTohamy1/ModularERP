using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Products.DTO
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "SKU is required")]
        [MaxLength(100, ErrorMessage = "SKU cannot exceed 100 characters")]
        public string SKU { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? PhotoUrl { get; set; }

        [Required(ErrorMessage = "Company ID is required")]
        public Guid CompanyId { get; set; }

        [Required(ErrorMessage = "Warehouse is required")]
        public Guid WarehouseId { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public Guid CategoryId { get; set; }

        public Guid? BrandId { get; set; }
        public Guid? SupplierId { get; set; }

        [MaxLength(100)]
        public string? Barcode { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Purchase price must be positive")]
        public decimal PurchasePrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Selling price must be positive")]
        public decimal SellingPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Minimum price must be positive")]
        public decimal? MinimumPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Discount { get; set; }

        [MaxLength(20)]
        public string? DiscountType { get; set; }

        public bool TrackStock { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? InitialStock { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? LowStockThreshold { get; set; }

        [MaxLength(2000)]
        public string? InternalNotes { get; set; }

        [MaxLength(500)]
        public string? Tags { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        public Guid? ItemGroupId { get; set; }
        public List<Guid>? TaxProfileIds { get; set; }
    }
}
