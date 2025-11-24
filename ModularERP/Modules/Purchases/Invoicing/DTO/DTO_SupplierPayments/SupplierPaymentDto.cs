namespace ModularERP.Modules.Purchases.Invoicing.DTO.DTO_SupplierPayments
{
    public class SupplierPaymentDto
    {
        public Guid Id { get; set; }
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public Guid? InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public Guid? PurchaseOrderId { get; set; }
        public string? PONumber { get; set; }

        public string PaymentNumber { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }

        public decimal Amount { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal UnallocatedAmount { get; set; }

        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }

        public string Status { get; set; } = string.Empty;
        public bool IsVoid { get; set; }
        public DateTime? VoidedAt { get; set; }
        public string? VoidedByName { get; set; }
        public string? VoidReason { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
