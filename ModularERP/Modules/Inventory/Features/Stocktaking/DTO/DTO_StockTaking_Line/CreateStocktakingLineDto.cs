namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Line
{
    public class CreateStocktakingLineDto
    {
        public Guid ProductId { get; set; }
        public Guid? UnitId { get; set; }
        public decimal PhysicalQty { get; set; }
        public string Note { get; set; }
        public string ImagePath { get; set; }
    }
}
