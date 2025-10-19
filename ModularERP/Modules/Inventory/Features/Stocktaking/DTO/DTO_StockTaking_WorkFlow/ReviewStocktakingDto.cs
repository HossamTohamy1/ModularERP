namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_WorkFlow
{
    public class ReviewStocktakingDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public string Status { get; set; }
        public int RecordedItems { get; set; }
        public decimal TotalVarianceValue { get; set; }
        public DateTime DateTime { get; set; }
    }
}
