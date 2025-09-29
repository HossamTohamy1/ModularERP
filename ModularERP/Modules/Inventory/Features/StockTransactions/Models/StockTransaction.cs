using ModularERP.Common.Models;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Models
{
    public class StockTransaction : BaseEntity
    {
        public Guid ProductId { get; set; }

        public Guid WarehouseId { get; set; }

        [Required]
        public StockTransactionType TransactionType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? UnitCost { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal StockLevelAfter { get; set; }

        [MaxLength(50)]
        public string? ReferenceType { get; set; }

        public Guid? ReferenceId { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public Guid TenantId { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual Product Product { get; set; } = null!;
        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual ApplicationUser? CreatedByUser { get; set; }
    }
}
