namespace ModularERP.Modules.Purchases.Refunds.DTO.DTO_DebitNote
{
    public class SupplierDebitNoteDto
    {
        public Guid Id { get; set; }
        public string DebitNoteNumber { get; set; } = string.Empty;
        public Guid RefundId { get; set; }
        public string RefundNumber { get; set; } = string.Empty;
        public DateTime NoteDate { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
