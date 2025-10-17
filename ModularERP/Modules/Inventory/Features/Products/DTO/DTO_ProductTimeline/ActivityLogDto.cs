namespace ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ProductTimeline
{
    public class ActivityLogDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public string? BeforeValues { get; set; }
        public string? AfterValues { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
