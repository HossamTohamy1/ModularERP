using ModularERP.Common.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;

namespace ModularERP.Modules.Purchases.Invoicing.Models
{
    public class InvoiceLineItem : BaseEntity
    {
        public Guid InvoiceId { get; set; }
        public Guid POLineItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }

        // Navigation Properties
        public virtual PurchaseInvoice Invoice { get; set; } = null!;
        public virtual POLineItem POLineItem { get; set; } = null!;
    }
}