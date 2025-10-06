using ModularERP.Modules.Inventory.Features.Products.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Models
{
    public class StockSnapshot
    {
        [Key]
        public Guid SnapshotId { get; set; }

        public Guid StocktakingId { get; set; }

        public Guid ProductId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,3)")]
        public decimal QtyAtStart { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        //public Guid TenantId { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual StocktakingHeader Stocktaking { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
