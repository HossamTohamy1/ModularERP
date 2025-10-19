namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Line
{
    public class StocktakingLineDto
    {
        public Guid Id { get; set; }
        public Guid StocktakingId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSKU { get; set; }
        public Guid? UnitId { get; set; }
        public string UnitName { get; set; }
        public decimal PhysicalQty { get; set; }
        public decimal SystemQtySnapshot { get; set; }
        public decimal? SystemQtyAtPost { get; set; }
        public decimal? VarianceQty { get; set; }
        public decimal? ValuationCost { get; set; }
        public decimal? VarianceValue { get; set; }
        public string Note { get; set; }
        public string ImagePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
