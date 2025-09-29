using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Products.Models
{
    public class ProductStats
    {
        [Key]
        public Guid ProductId { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal TotalSold { get; set; } = 0;

        [Column(TypeName = "decimal(18,3)")]
        public decimal SoldLast28Days { get; set; } = 0;

        [Column(TypeName = "decimal(18,3)")]
        public decimal SoldLast7Days { get; set; } = 0;

        [Column(TypeName = "decimal(18,3)")]
        public decimal OnHandStock { get; set; } = 0;

        [Column(TypeName = "decimal(18,4)")]
        public decimal AvgUnitCost { get; set; } = 0;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public virtual Product Product { get; set; } = null!;
    }
}
