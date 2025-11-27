namespace ModularERP.Modules.Purchases.Invoicing.DTO.DTO_SupplierPayments
{
    public class SupplierPaymentDto
    {
        public Guid Id { get; set; }

        // Supplier Info
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;

        // Invoice/PO Info
        public Guid? InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public Guid? PurchaseOrderId { get; set; }
        public string? PONumber { get; set; }

        // Payment Details
        public string PaymentNumber { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty; // AgainstInvoice, Deposit, Advance

        // ✅ PaymentMethod as Entity (not enum)
        public Guid PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; } = string.Empty;
        public string PaymentMethodCode { get; set; } = string.Empty;
        public bool PaymentMethodRequiresReference { get; set; }

        public DateTime PaymentDate { get; set; }

        // Amounts
        public decimal Amount { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal UnallocatedAmount { get; set; }

        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }

        // Status
        public string Status { get; set; } = string.Empty;

        // Void Info
        public bool IsVoid { get; set; }
        public DateTime? VoidedAt { get; set; }
        public string? VoidedByName { get; set; }
        public string? VoidReason { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}