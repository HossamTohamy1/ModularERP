using ModularERP.Common.Models;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.Services.Models
{
    public class Service : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Code { get; set; }

        public string? Description { get; set; }

        [MaxLength(500)]
        public string? Photo { get; set; }

        public Guid? CategoryId { get; set; }

        public Guid? SupplierId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? PurchasePrice { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? MinPrice { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? Discount { get; set; }

        public DiscountType? DiscountType { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal? ProfitMargin { get; set; }

        public string? InternalNotes { get; set; }

        [MaxLength(500)]
        public string? Tags { get; set; }

        public ServiceStatus Status { get; set; } = ServiceStatus.Active;

        public Guid TenantId { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual Category? Category { get; set; }
        public virtual Supplier? Supplier { get; set; }
        public virtual ICollection<ServiceTaxProfile> ServiceTaxProfiles { get; set; } = new List<ServiceTaxProfile>();
    }
}
