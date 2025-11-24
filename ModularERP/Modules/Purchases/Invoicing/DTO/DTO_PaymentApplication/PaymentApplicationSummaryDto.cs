namespace ModularERP.Modules.Purchases.Invoicing.DTO.DTO_PaymentApplication
{
    public class PaymentApplicationSummaryDto
    {
        public Guid PaymentId { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal UnallocatedAmount { get; set; }
        public int AllocationsCount { get; set; }
        public List<PaymentAllocationResponseDto> Allocations { get; set; } = new();
    }
}
