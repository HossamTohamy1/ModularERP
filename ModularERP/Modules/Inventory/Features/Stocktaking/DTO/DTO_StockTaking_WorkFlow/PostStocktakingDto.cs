namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_WorkFlow
{
    public class PostStocktakingDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public string Status { get; set; }
        public DateTime PostedAt { get; set; }
        public int AdjustmentsPosted { get; set; }
        public decimal TotalAdjustmentValue { get; set; }
    }
}
