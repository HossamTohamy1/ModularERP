using ModularERP.Common.Models;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Models
{
    public class PONote : BaseEntity
    {
        public Guid PurchaseOrderId { get; set; }
        public string NoteText { get; set; } = string.Empty;

        // Navigation Properties
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
    }
}