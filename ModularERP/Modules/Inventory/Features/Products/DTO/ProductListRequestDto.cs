using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Products.DTO
{
    public class ProductListRequestDto
    {
        public string? SearchTerm { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? BrandId { get; set; }
        public string? Status { get; set; }
        public bool? TrackStock { get; set; }
        public string? StockStatus { get; set; } // InStock, OutOfStock, LowStock
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "desc";

        [Required(ErrorMessage = "Company ID is required")]
        public Guid CompanyId { get; set; }
    }
}
