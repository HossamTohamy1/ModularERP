using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Modules.Finance.Features.Currencies.Models;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Modules.Purchases.WorkFlow.Models;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Models
{
    public class PurchaseOrder : BaseEntity
    {
        public string PONumber { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public Guid SupplierId { get; set; }
        public string CurrencyCode { get; set; } = "SAR";
        public DateTime PODate { get; set; } = DateTime.UtcNow;
        public string? PaymentTerms { get; set; }

        // Amounts
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal AdjustmentAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal TotalPaid { get; set; } // ⭐ NEW: Total payments made (deposits + regular payments)
        public decimal AmountDue { get; set; }

        // Status Tracking
        public string ReceptionStatus { get; set; } = "NotReceived";
        public string PaymentStatus { get; set; } = "Unpaid";
        public string DocumentStatus { get; set; } = "Draft";

        // Workflow
        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? SubmittedBy { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public Guid? ClosedBy { get; set; }
        public DateTime? ClosedAt { get; set; }

        public string? Notes { get; set; }
        public string? Terms { get; set; }

        // Navigation Properties
        public virtual Company Company { get; set; } = null!;
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual Currency Currency { get; set; } = null!;
        public virtual ApplicationUser? ApprovedByUser { get; set; }
        public virtual ApplicationUser? SubmittedByUser { get; set; }
        public virtual ApplicationUser? ClosedByUser { get; set; }

        public virtual ICollection<POLineItem> LineItems { get; set; } = new List<POLineItem>();
        public virtual ICollection<PODeposit> Deposits { get; set; } = new List<PODeposit>();
        public virtual ICollection<POShippingCharge> ShippingCharges { get; set; } = new List<POShippingCharge>();
        public virtual ICollection<PODiscount> Discounts { get; set; } = new List<PODiscount>();
        public virtual ICollection<POAdjustment> Adjustments { get; set; } = new List<POAdjustment>();
        public virtual ICollection<POAttachment> Attachments { get; set; } = new List<POAttachment>();
        public virtual ICollection<PONote> PONotes { get; set; } = new List<PONote>();
        public virtual ICollection<GoodsReceiptNote> GoodsReceipts { get; set; } = new List<GoodsReceiptNote>();
        public virtual ICollection<PurchaseInvoice> Invoices { get; set; } = new List<PurchaseInvoice>();
        public virtual ICollection<PurchaseRefund> Refunds { get; set; } = new List<PurchaseRefund>();
        public virtual ICollection<POApprovalHistory> ApprovalHistory { get; set; } = new List<POApprovalHistory>();
        public virtual ICollection<POAuditLog> AuditLogs { get; set; } = new List<POAuditLog>();
        public virtual ICollection<SupplierPayment> Payments { get; set; } = new List<SupplierPayment>(); // ⭐ NEW
    }
}