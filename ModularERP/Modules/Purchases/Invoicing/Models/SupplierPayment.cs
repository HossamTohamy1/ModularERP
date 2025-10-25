using ModularERP.Common.Models;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;

namespace ModularERP.Modules.Purchases.Invoicing.Models
{
    public class SupplierPayment : BaseEntity
    {
        public Guid SupplierId { get; set; }
        public Guid? InvoiceId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }

        // Navigation Properties
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual PurchaseInvoice? Invoice { get; set; }
    }
}
