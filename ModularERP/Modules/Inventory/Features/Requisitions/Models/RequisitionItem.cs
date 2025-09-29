using ModularERP.Common.Models;
using ModularERP.Modules.Inventory.Features.Products.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Models
{
    public class RequisitionItem : BaseEntity
    {
        public Guid RequisitionId { get; set; }

        public Guid ProductId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? UnitPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? StockOnHand { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? NewStockOnHand { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? LineTotal { get; set; }

        // Navigation Properties
        public virtual Requisition Requisition { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
