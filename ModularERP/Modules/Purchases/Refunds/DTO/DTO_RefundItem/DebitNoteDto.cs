namespace ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundItem
{
    public class DebitNoteDto
    {
        public Guid Id { get; set; }
        public string DebitNoteNumber { get; set; } = string.Empty;
        public Guid RefundId { get; set; }
        public string RefundNumber { get; set; } = string.Empty;
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public DateTime NoteDate { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
