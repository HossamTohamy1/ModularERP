using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Models
{
    public class RequisitionApprovalLog : BaseEntity
    {
        public Guid RequisitionId { get; set; }

        [Required]
        public RequisitionAction Action { get; set; }

        public Guid? UserId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? Comments { get; set; }

        // Navigation Properties
        public virtual Requisition Requisition { get; set; } = null!;
        public virtual ApplicationUser? User { get; set; }
    }
}
