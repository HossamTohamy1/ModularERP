namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceList
{
    public class PriceListItemListDto
    {
        public Guid Id { get; set; }
        public Guid PriceListId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public decimal? BasePrice { get; set; }
        public decimal? ListPrice { get; set; }
        public decimal? DiscountValue { get; set; }
        public string? DiscountType { get; set; }
        public decimal? FinalPrice { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; }
    }
}
