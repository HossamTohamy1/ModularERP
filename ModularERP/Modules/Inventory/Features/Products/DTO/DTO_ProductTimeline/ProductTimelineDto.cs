namespace ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ProductTimeline
{
    public class ProductTimelineDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string? ItemReference { get; set; }
        public Guid? UserId { get; set; }
        public decimal? StockBalance { get; set; }
        public decimal? AveragePrice { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
