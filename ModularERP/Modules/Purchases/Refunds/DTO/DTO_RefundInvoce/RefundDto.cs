namespace ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce
{
    public class RefundDto
    {
        public Guid Id { get; set; }
        public string RefundNumber { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public string? PurchaseOrderNumber { get; set; }
        public Guid SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public DateTime RefundDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public bool HasDebitNote { get; set; }
        public DebitNoteDto? DebitNote { get; set; }
        public List<RefundLineItemDto> LineItems { get; set; } = new();

        // Enhanced Response
        public StatusUpdatesDto? StatusUpdates { get; set; }
        public List<InventoryImpactDto>? InventoryImpact { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
