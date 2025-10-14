using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition
{
    public class RequisitionListDto
    {
        public Guid Id { get; set; }
        public RequisitionType Type { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string? SupplierName { get; set; }
        public RequisitionStatus Status { get; set; }
        public decimal ItemsTotal { get; set; }
        public int ItemsCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
    }
}
