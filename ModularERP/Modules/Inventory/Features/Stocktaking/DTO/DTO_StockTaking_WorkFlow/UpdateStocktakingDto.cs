namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_WorkFlow
{
    public class UpdateStocktakingDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public string Notes { get; set; }
        public bool UpdateSystem { get; set; }
        public string Status { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
