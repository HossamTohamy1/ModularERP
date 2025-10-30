namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POStatus
{
    public class POActivityLogDto
    {
        public Guid Id { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
        public DateTime PerformedAt { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
    }
}
