using ModularERP.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Products.Models
{
    public class ProductTimeline : BaseEntity
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ActionType { get; set; } = string.Empty;

        // مثال: Purchase, Sale, Adjustment, Transfer
        [MaxLength(200)]
        public string? ItemReference { get; set; }

        // مثال: Order ID, Invoice ID, Stock Take ID
        public Guid? UserId { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? StockBalance { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? AveragePrice { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation Properties
        public virtual Product? Product { get; set; }
    }
}
