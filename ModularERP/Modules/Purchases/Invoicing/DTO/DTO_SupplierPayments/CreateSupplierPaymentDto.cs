namespace ModularERP.Modules.Purchases.Invoicing.DTO.DTO_SupplierPayments
{
    public class CreateSupplierPaymentDto
    {
        public Guid SupplierId { get; set; }
        public Guid? InvoiceId { get; set; }
        public Guid? PurchaseOrderId { get; set; }

        public string PaymentType { get; set; } = "AgainstInvoice"; // AgainstInvoice, Advance, Refund
        public string PaymentMethod { get; set; } = string.Empty; // Cash, Bank, Cheque, Card
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public decimal Amount { get; set; }
        public decimal? AllocatedAmount { get; set; } // Optional: if not provided, auto-allocate

        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }
}
