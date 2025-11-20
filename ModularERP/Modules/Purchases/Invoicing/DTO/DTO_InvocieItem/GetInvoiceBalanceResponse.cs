namespace ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem
{
    public class GetInvoiceBalanceResponse
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DepositApplied { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal AmountDue { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public int DaysOverdue { get; set; }
        public bool IsOverdue { get; set; }
        public SupplierInfoDto Supplier { get; set; } = new();
    }
    public class SupplierInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
