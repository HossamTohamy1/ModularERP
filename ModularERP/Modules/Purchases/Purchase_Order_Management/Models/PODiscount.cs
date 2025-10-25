using ModularERP.Common.Models;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Models
{
    public class PODiscount : BaseEntity
    {
        public Guid PurchaseOrderId { get; set; }
        public string DiscountType { get; set; } = "Percentage"; // Percentage, Fixed
        public decimal DiscountValue { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? Description { get; set; }

        // Navigation Properties
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
    }
}
