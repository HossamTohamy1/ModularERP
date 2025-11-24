using ModularERP.Common.Models;

namespace ModularERP.Modules.Purchases.Invoicing.Models
{
    public class PaymentAllocation : BaseEntity
    {
        /// <summary>
        /// Foreign key to SupplierPayment
        /// </summary>
        public Guid PaymentId { get; set; }

        /// <summary>
        /// Foreign key to PurchaseInvoice
        /// </summary>
        public Guid InvoiceId { get; set; }

        /// <summary>
        /// Amount allocated from payment to this specific invoice
        /// Must be > 0 and <= Invoice.AmountDue at time of allocation
        /// </summary>
        public decimal AllocatedAmount { get; set; }

        /// <summary>
        /// Date when this allocation was made
        /// </summary>
        public DateTime AllocationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional notes about this specific allocation
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// User who created this allocation
        /// </summary>
        public string? AllocatedBy { get; set; }

        /// <summary>
        /// Indicates if this allocation has been voided/reversed
        /// </summary>
        public bool IsVoided { get; set; } = false;

        /// <summary>
        /// Date when allocation was voided
        /// </summary>
        public DateTime? VoidedAt { get; set; }

        /// <summary>
        /// User who voided this allocation
        /// </summary>
        public Guid? VoidedBy { get; set; }

        /// <summary>
        /// Reason for voiding
        /// </summary>
        public string? VoidReason { get; set; }

        // ========================================
        // Navigation Properties
        // ========================================

        /// <summary>
        /// The payment this allocation belongs to
        /// </summary>
        public virtual SupplierPayment Payment { get; set; } = null!;

        /// <summary>
        /// The invoice this allocation is applied to
        /// </summary>
        public virtual PurchaseInvoice Invoice { get; set; } = null!;

        /// <summary>
        /// User who voided the allocation
        /// </summary>
        public virtual ApplicationUser? VoidedByUser { get; set; }
    }
}
