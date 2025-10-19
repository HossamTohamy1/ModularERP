using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header
{
    public class UpdateStocktakingDto
    {
        public Guid Id { get; set; }
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string? Notes { get; set; }
        public StocktakingStatus Status { get; set; }
        public bool UpdateSystem { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
