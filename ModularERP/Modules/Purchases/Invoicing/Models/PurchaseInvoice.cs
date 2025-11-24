using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Modules.Purchases.Refunds.Models;

namespace ModularERP.Modules.Purchases.Invoicing.Models
{
    public class PurchaseInvoice : BaseEntity
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public Guid PurchaseOrderId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid SupplierId { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DepositApplied { get; set; }
        public decimal AmountDue { get; set; }
        public string PaymentStatus { get; set; } = "Unpaid"; // Unpaid, PartiallyPaid, PaidInFull
        public string? Notes { get; set; }

        // Navigation Properties
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
        public virtual Company Company { get; set; } = null!;
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual ICollection<PurchaseRefund> Refunds { get; set; } = new List<PurchaseRefund>();
        public virtual ICollection<PaymentAllocation> PaymentAllocations { get; set; } = new List<PaymentAllocation>();

        public virtual ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
        public virtual ICollection<SupplierPayment> Payments { get; set; } = new List<SupplierPayment>();
    }
}
