using ModularERP.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Products.Models
{
    public class ItemGroupItem : BaseEntity
    {
        public Guid GroupId { get; set; }

        public Guid ProductId { get; set; }

        [MaxLength(100)]
        public string? SKU { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? PurchasePrice { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? SellingPrice { get; set; }

        [MaxLength(100)]
        public string? Barcode { get; set; }

        // Navigation Properties
        public virtual ItemGroup Group { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
