using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Product : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? SKU { get; set; }

    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Photo { get; set; }

    // إضافة CompanyId هنا
    public Guid CompanyId { get; set; }

    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? SupplierId { get; set; }

    [MaxLength(100)]
    public string? Barcode { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? PurchasePrice { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? SellingPrice { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? MinPrice { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? Discount { get; set; }

    public DiscountType? DiscountType { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal? ProfitMargin { get; set; }

    public bool TrackStock { get; set; } = true;

    [Column(TypeName = "decimal(18,3)")]
    public decimal? InitialStock { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal? LowStockThreshold { get; set; }

    public string? InternalNotes { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.Active;

    // Navigation Properties
    public virtual Company? Company { get; set; }  
    public virtual Category? Category { get; set; }
    public virtual Brand? Brand { get; set; }
    public virtual Supplier? Supplier { get; set; }
    public virtual ICollection<ProductTaxProfile> ProductTaxProfiles { get; set; } = new List<ProductTaxProfile>();
    public virtual ICollection<ItemGroupItem> ItemGroupItems { get; set; } = new List<ItemGroupItem>();
    public virtual ICollection<ProductCustomFieldValue> CustomFieldValues { get; set; } = new List<ProductCustomFieldValue>();
}