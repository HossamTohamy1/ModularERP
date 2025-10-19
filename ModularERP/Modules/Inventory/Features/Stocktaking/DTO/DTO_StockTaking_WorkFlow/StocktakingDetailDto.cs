namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_WorkFlow
{
    public class StocktakingDetailDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; }
        public DateTime DateTime { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public bool UpdateSystem { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<StocktakingLineDto> Lines { get; set; } = new List<StocktakingLineDto>();
    }
}
