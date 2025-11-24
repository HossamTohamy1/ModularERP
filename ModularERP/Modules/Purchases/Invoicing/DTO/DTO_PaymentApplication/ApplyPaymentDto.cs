namespace ModularERP.Modules.Purchases.Invoicing.DTO.DTO_PaymentApplication
{
    public class ApplyPaymentDto
    {
        public Guid PaymentId { get; set; }
        public List<InvoiceAllocationDto> Allocations { get; set; } = new();
        public string? Notes { get; set; }
    }
    public class InvoiceAllocationDto
    {
        public Guid InvoiceId { get; set; }
        public decimal AllocatedAmount { get; set; }
        public string? Notes { get; set; }
    }
}
