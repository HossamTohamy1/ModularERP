namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_ImportExport
{
    public class ImportedLineDto
    {
        public Guid LineId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public decimal PhysicalQty { get; set; }
        public decimal SystemQtySnapshot { get; set; }
        public decimal? VarianceQty { get; set; }
        public string? Note { get; set; }
    }
}
