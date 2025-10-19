namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_WorkFlow
{
    public class StocktakingListDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string Status { get; set; }
        public int LineCount { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByName { get; set; }
    }
}
