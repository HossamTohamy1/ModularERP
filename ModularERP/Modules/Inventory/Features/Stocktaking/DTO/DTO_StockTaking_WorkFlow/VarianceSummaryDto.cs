namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_WorkFlow
{
    public class VarianceSummaryDto
    {
        public Guid StocktakingId { get; set; }
        public string Number { get; set; }
        public string Status { get; set; }
        public int TotalRecordedItems { get; set; }
        public int TotalShortages { get; set; }
        public int TotalOverages { get; set; }
        public decimal TotalShortageQty { get; set; }
        public decimal TotalOverageQty { get; set; }
        public decimal TotalShortageValue { get; set; }
        public decimal TotalOverageValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByName { get; set; }
    }
}
