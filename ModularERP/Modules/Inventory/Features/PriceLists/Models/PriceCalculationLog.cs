using ModularERP.Common.Models;
using ModularERP.Modules.Inventory.Features.Products.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Models
{
    public class PriceCalculationLog : BaseEntity
    {
        public Guid? TransactionId { get; set; }

        [MaxLength(50)]
        public string? TransactionType { get; set; }

        public Guid? ProductId { get; set; }

        [MaxLength(100)]
        public string? AppliedRule { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? ValueBefore { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? ValueAfter { get; set; }

        public Guid? UserId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Product? Product { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
