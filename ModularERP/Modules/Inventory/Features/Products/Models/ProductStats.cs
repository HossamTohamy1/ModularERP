using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Features.Companys.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModularERP.Modules.Inventory.Features.Products.Models
{
    public class ProductStats : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Guid CompanyId { get; set; }

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

        // Navigation Properties
        public virtual Product Product { get; set; } = null!;
        public virtual Company? Company { get; set; }
    }
}