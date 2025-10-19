namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header
{
    public class StocktakingLineDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal PhysicalQty { get; set; }
        public decimal SystemQtySnapshot { get; set; }
        public decimal? SystemQtyAtPost { get; set; }
        public decimal? VarianceQty { get; set; }
        public decimal? ValuationCost { get; set; }
        public decimal? VarianceValue { get; set; }
        public string? Note { get; set; }
        public string? ImagePath { get; set; }
    }
}
