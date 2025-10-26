namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder
{
    public class PONoteDto
    {
        public Guid Id { get; set; }
        public string NoteText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
