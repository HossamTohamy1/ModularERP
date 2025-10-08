namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceList
{
    public class PriceListItemDto
    {
        public Guid Id { get; set; }
        public Guid PriceListId { get; set; }
        public Guid? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductSKU { get; set; }
        public Guid? ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public string? ServiceCode { get; set; }
        public decimal? BasePrice { get; set; }
        public decimal? ListPrice { get; set; }
        public decimal? DiscountValue { get; set; }
        public string? DiscountType { get; set; }
        public decimal? FinalPrice { get; set; }
        public Guid? TaxProfileId { get; set; }
        public string? TaxProfileName { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
