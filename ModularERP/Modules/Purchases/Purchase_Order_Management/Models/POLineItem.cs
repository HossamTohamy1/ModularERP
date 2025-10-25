using ModularERP.Common.Models;
using ModularERP.Modules.Inventory.Features.Services.Models;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Modules.Purchases.Refunds.Models;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Models
{
    public class POLineItem : BaseEntity
    {
        public Guid PurchaseOrderId { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? ServiceId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public Guid? TaxProfileId { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }

        // Tracking
        public decimal ReceivedQuantity { get; set; }
        public decimal InvoicedQuantity { get; set; }
        public decimal ReturnedQuantity { get; set; }
        public decimal RemainingQuantity { get; set; }

        // Navigation Properties
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
        public virtual Product? Product { get; set; }
        public virtual Service? Service { get; set; }
        public virtual TaxProfile? TaxProfile { get; set; }

        public virtual ICollection<GRNLineItem> GRNLineItems { get; set; } = new List<GRNLineItem>();
        public virtual ICollection<InvoiceLineItem> InvoiceLineItems { get; set; } = new List<InvoiceLineItem>();
        public virtual ICollection<RefundLineItem> RefundLineItems { get; set; } = new List<RefundLineItem>();
    }
}
