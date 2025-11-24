namespace ModularERP.Modules.Purchases.Invoicing.DTO.DTO_PaymentApplication
{
    public class PayInvoiceDto
    {
        public Guid InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // Cash, Bank, Cheque, Card
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }
}
