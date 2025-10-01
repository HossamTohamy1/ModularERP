using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Models
{
    public class Requisition: BaseEntity
    {
        [Required]
        public RequisitionType Type { get; set; }

        [Required]
        [MaxLength(50)]
        public string Number { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        public Guid WarehouseId { get; set; }

        public Guid? JournalAccountId { get; set; }

        public Guid? SupplierId { get; set; }

        public string? Notes { get; set; }

        [MaxLength(500)]
        public string? Attachments { get; set; }

        public RequisitionStatus Status { get; set; } = RequisitionStatus.Draft;
        public Guid CompanyId { get; set; }

        // Workflow tracking
        public Guid? SubmittedBy { get; set; }
        public DateTime? SubmittedAt { get; set; }

        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public Guid? ConfirmedBy { get; set; }
        public DateTime? ConfirmedAt { get; set; }

        public Guid? ReversedBy { get; set; }
        public DateTime? ReversedAt { get; set; }

        public Guid? ParentRequisitionId { get; set; }

        public Guid TenantId { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual Supplier? Supplier { get; set; }
        public virtual Requisition? ParentRequisition { get; set; }
        public virtual Company? Company { get; set; }

        public virtual ApplicationUser? SubmittedByUser { get; set; }
        public virtual ApplicationUser? ApprovedByUser { get; set; }
        public virtual ApplicationUser? ConfirmedByUser { get; set; }
        public virtual ApplicationUser? ReversedByUser { get; set; }

        public virtual ICollection<RequisitionItem> Items { get; set; } = new List<RequisitionItem>();
        public virtual ICollection<RequisitionApprovalLog> ApprovalLogs { get; set; } = new List<RequisitionApprovalLog>();
        public virtual ICollection<Requisition> ChildRequisitions { get; set; } = new List<Requisition>();
    }
}
