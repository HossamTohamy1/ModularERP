using ModularERP.Common.Models;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;

namespace ModularERP.Modules.Purchases.Invoicing.Models
{
    public class SupplierPayment : BaseEntity
    {
        public Guid SupplierId { get; set; }
        public Guid? InvoiceId { get; set; }
        public Guid? PurchaseOrderId { get; set; }

        public string PaymentNumber { get; set; } = string.Empty; // Auto-generated unique number
        public string PaymentType { get; set; } = "AgainstInvoice"; // AgainstInvoice, Advance, Refund
        public string PaymentMethod { get; set; } = string.Empty; // Cash, Bank, Cheque, Card
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        // Amounts
        public decimal Amount { get; set; } // Total payment amount
        public decimal AllocatedAmount { get; set; } // Amount allocated to invoice(s)
        public decimal UnallocatedAmount { get; set; } // Remaining balance (Advance)

        public string? ReferenceNumber { get; set; } // Bank ref, Cheque number, etc.
        public string? Notes { get; set; }

        // Status & Control
        public string Status { get; set; } = "Draft"; // Draft, Posted, Void
        public bool IsVoid { get; set; } = false;
        public Guid? VoidedBy { get; set; }
        public DateTime? VoidedAt { get; set; }
        public string? VoidReason { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation Properties
        public virtual ICollection<PaymentAllocation> Allocations { get; set; } = new List<PaymentAllocation>();

        public virtual Supplier Supplier { get; set; } = null!;
        public virtual PurchaseInvoice? Invoice { get; set; }
        public virtual PurchaseOrder? PurchaseOrder { get; set; }
        public virtual ApplicationUser? VoidedByUser { get; set; }
    }
}