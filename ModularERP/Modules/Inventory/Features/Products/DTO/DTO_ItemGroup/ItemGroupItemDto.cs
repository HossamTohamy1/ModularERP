namespace ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ItemGroup
{
    public class ItemGroupItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? SKU { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? SellingPrice { get; set; }
        public string? Barcode { get; set; }
    }
}
