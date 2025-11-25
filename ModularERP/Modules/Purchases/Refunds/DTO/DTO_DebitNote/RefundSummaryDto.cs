namespace ModularERP.Modules.Purchases.Refunds.DTO.DTO_DebitNote
{
    public class RefundSummaryDto
    {
        public Guid Id { get; set; }
        public string RefundNumber { get; set; } = string.Empty;
        public DateTime RefundDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Reason { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public string PONumber { get; set; } = string.Empty;
    }
}
