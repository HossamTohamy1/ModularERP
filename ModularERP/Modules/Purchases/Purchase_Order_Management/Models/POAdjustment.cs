using ModularERP.Common.Models;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Models
{
    public class POAdjustment : BaseEntity
    {
        public Guid PurchaseOrderId { get; set; }
        public string AdjustmentType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Notes { get; set; }

        // Navigation Properties
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
    }
}
