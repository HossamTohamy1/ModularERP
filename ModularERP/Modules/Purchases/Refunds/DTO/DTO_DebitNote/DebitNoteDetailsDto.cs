namespace ModularERP.Modules.Purchases.Refunds.DTO.DTO_DebitNote
{
    public class DebitNoteDetailsDto
    {
        public Guid Id { get; set; }
        public string DebitNoteNumber { get; set; } = string.Empty;
        public Guid RefundId { get; set; }
        public string RefundNumber { get; set; } = string.Empty;
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string? SupplierEmail { get; set; }
        public string? SupplierPhone { get; set; }
        public DateTime NoteDate { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Refund Details
        public RefundSummaryDto? Refund { get; set; }
    }
}
