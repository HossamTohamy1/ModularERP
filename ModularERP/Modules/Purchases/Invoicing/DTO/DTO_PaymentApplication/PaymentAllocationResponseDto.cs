namespace ModularERP.Modules.Purchases.Invoicing.DTO.DTO_PaymentApplication
{
    public class PaymentAllocationResponseDto
    {
        public Guid Id { get; set; }
        public Guid PaymentId { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal AllocatedAmount { get; set; }
        public DateTime AllocationDate { get; set; }
        public string? Notes { get; set; }
        public string? AllocatedBy { get; set; }
        public bool IsVoided { get; set; }
        public DateTime? VoidedAt { get; set; }
        public string? VoidReason { get; set; }
    }
}