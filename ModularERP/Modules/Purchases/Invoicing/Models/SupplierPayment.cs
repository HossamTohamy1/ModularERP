using ModularERP.Common.Enum.Purchases_Enum;
using ModularERP.Common.Models;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Modules.Purchases.Payment.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using System.ComponentModel.DataAnnotations;


namespace ModularERP.Modules.Purchases.Invoicing.Models
{
    public class SupplierPayment : BaseEntity
    {
        public Guid SupplierId { get; set; }
        public Guid? InvoiceId { get; set; }
        public Guid? PurchaseOrderId { get; set; }

        [MaxLength(50)]
        public string PaymentNumber { get; set; } = string.Empty;

        public PaymentType PaymentType { get; set; } = PaymentType.AgainstInvoice;

        [Required]
        public Guid PaymentMethodId { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        // Amounts
        public decimal Amount { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal UnallocatedAmount { get; set; }

        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }

        public string? Notes { get; set; }

        public SupplierPaymentStatus Status { get; set; } = SupplierPaymentStatus.Draft;

        public bool IsVoid { get; set; } = false;
        public Guid? VoidedBy { get; set; }
        public DateTime? VoidedAt { get; set; }
        public string? VoidReason { get; set; }

        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation Properties
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual PurchaseInvoice? Invoice { get; set; }
        public virtual PurchaseOrder? PurchaseOrder { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; } = null!; 
        public virtual ApplicationUser? VoidedByUser { get; set; }
        public virtual ICollection<PaymentAllocation> Allocations { get; set; } = new List<PaymentAllocation>();
    }
}