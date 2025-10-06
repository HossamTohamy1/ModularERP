using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Models
{
    public class StocktakingHeader : BaseEntity
    {
        public Guid WarehouseId { get; set; }
        public Guid CompanyId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Number { get; set; } = string.Empty;

        [Required]
        public DateTime DateTime { get; set; }

        public string? Notes { get; set; }

        public StocktakingStatus Status { get; set; } = StocktakingStatus.Draft;

        /// <summary>
        /// Whether to post adjustments to inventory after approval
        /// </summary>
        public bool UpdateSystem { get; set; } = true;

        // Workflow tracking
        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public Guid? PostedBy { get; set; }
        public DateTime? PostedAt { get; set; }

        //public Guid TenantId { get; set; }
        

        // Navigation Properties
        public virtual Company? Company { get; set; }

        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual ApplicationUser? ApprovedByUser { get; set; }
        public virtual ApplicationUser? PostedByUser { get; set; }

        public virtual ICollection<StocktakingLine> Lines { get; set; } = new List<StocktakingLine>();
        public virtual ICollection<StocktakingAttachment> Attachments { get; set; } = new List<StocktakingAttachment>();
        public virtual ICollection<StockSnapshot> Snapshots { get; set; } = new List<StockSnapshot>();
    }
}
