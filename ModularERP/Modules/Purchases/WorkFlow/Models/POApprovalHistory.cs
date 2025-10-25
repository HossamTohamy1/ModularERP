using ModularERP.Common.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;

namespace ModularERP.Modules.Purchases.WorkFlow.Models
{
    public class POApprovalHistory : BaseEntity
    {
        public Guid PurchaseOrderId { get; set; }
        public Guid ApprovedBy { get; set; }
        public DateTime ApprovalDate { get; set; } = DateTime.UtcNow;
        public string Action { get; set; } = string.Empty; // Approved, Rejected
        public string? Comments { get; set; }

        // Navigation Properties
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
        public virtual ApplicationUser ApprovedByUser { get; set; } = null!;
    }
}