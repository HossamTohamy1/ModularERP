namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POStatus
{
    public class POTimelineDto
    {
        public Guid PurchaseOrderId { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public List<TimelineEventDto> Events { get; set; } = new();
    }
}
