using ModularERP.Common.Models;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;

namespace ModularERP.Modules.Purchases.Refunds.Models
{
    public class PurchaseRefund : BaseEntity
    {
        public string RefundNumber { get; set; } = string.Empty;
        public Guid PurchaseOrderId { get; set; }
        public Guid SupplierId { get; set; }
        public DateTime RefundDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }

        // Navigation Properties
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual DebitNote? DebitNote { get; set; }

        public virtual ICollection<RefundLineItem> LineItems { get; set; } = new List<RefundLineItem>();
    }
}
