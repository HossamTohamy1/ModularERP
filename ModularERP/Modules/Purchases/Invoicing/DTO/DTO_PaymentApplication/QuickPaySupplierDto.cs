namespace ModularERP.Modules.Purchases.Invoicing.DTO.DTO_PaymentApplication
{
    public class QuickPaySupplierDto
    {
        public Guid SupplierId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; } = "Advance"; // AgainstInvoice, Advance
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public List<InvoiceAllocationDto>? InvoiceAllocations { get; set; }
    }
}
