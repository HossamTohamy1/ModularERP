namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_ImportExport
{
    public class ImportLineItemDto
    {
        public string ProductCode { get; set; } = string.Empty;
        public decimal PhysicalQty { get; set; }
        public string? Note { get; set; }
        public string? ImageUrl { get; set; }
    }
}
