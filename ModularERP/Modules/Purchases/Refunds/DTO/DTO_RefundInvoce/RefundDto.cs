namespace ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce
{
    public class RefundDto
    {
        public Guid Id { get; set; }
        public string RefundNumber { get; set; } = string.Empty;
        public Guid PurchaseOrderId { get; set; }
        public string PurchaseOrderNumber { get; set; } = string.Empty;
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public DateTime RefundDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public bool HasDebitNote { get; set; }
        public DebitNoteDto? DebitNote { get; set; }
        public List<RefundLineItemDto> LineItems { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
