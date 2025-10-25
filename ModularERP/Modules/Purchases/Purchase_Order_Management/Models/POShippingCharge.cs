using ModularERP.Common.Models;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Models
{
    public class POShippingCharge : BaseEntity
    {
        public Guid PurchaseOrderId { get; set; }
        public decimal ShippingFee { get; set; }
        public Guid? TaxProfileId { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public string? Description { get; set; }

        // Navigation Properties
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
        public virtual TaxProfile? TaxProfile { get; set; }
    }
}
