namespace ModularERP.Modules.Purchases.Refunds.DTO.DTO_DebitNote
{
    public class CreateDebitNoteDto
    {
        public Guid RefundId { get; set; }
        public string? Notes { get; set; }
    }
}
