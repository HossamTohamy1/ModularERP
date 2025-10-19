using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header
{
    public class StocktakingDetailDto
    {
        public Guid Id { get; set; }
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string? Notes { get; set; }
        public StocktakingStatus Status { get; set; }
        public bool UpdateSystem { get; set; }
        public Guid? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? PostedBy { get; set; }
        public string? PostedByName { get; set; }
        public DateTime? PostedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<StocktakingLineDto> Lines { get; set; } = new();
    }
}
