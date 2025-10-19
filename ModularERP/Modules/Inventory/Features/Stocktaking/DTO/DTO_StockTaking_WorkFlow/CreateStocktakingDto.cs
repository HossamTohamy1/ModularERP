namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_WorkFlow
{
    public class CreateStocktakingDto
    {
        public Guid Id { get; set; }
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string Number { get; set; }
        public DateTime DateTime { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
