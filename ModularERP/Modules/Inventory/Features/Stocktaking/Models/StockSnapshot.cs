using ModularERP.Modules.Inventory.Features.Products.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ModularERP.Common.Models;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Models
{
    public class StockSnapshot : BaseEntity
    {
        [Key]
        public Guid SnapshotId { get; set; }

        public Guid StocktakingId { get; set; }

        public Guid ProductId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,3)")]
        public decimal QtyAtStart { get; set; }


        // Navigation Properties
        public virtual StocktakingHeader Stocktaking { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
