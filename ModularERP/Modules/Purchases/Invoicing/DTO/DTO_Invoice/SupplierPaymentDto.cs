namespace ModularERP.Modules.Purchases.Invoicing.DTO.DTO_Invoice
{
    public class SupplierPaymentDto
    {
        public Guid Id { get; set; }
        public Guid SupplierId { get; set; }
        public Guid? InvoiceId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }
}
