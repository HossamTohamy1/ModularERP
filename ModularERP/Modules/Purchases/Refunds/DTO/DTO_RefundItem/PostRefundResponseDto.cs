namespace ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundItem
{
    public class PostRefundResponseDto
    {
        public Guid RefundId { get; set; }
        public string RefundNumber { get; set; } = string.Empty;
        public Guid DebitNoteId { get; set; }
        public string DebitNoteNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime PostedDate { get; set; }
        public bool InventoryUpdated { get; set; }
        public bool AccountsUpdated { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
