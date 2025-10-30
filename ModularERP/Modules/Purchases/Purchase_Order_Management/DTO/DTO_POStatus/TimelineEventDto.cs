namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POStatus
{
    public class TimelineEventDto
    {
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = string.Empty; // Created, Approved, Received, Paid, Closed
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
