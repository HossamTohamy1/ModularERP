using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header
{
    public class StocktakingListDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public StocktakingStatus Status { get; set; }
        public bool UpdateSystem { get; set; }
        public int LineCount { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
