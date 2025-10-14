using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition
{
    public class RequisitionResponseDto
    {
        public Guid Id { get; set; }
        public RequisitionType Type { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public Guid? JournalAccountId { get; set; }
        public Guid? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? Notes { get; set; }
        public RequisitionStatus Status { get; set; }
        public Guid CompanyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;

        public Guid? SubmittedBy { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ConfirmedBy { get; set; }
        public DateTime? ConfirmedAt { get; set; }

        public decimal ItemsTotal { get; set; }
        public int ItemsCount { get; set; }

        public List<RequisitionItemDto> Items { get; set; } = new();
        public List<RequisitionAttachmentResponseDto> Attachments { get; set; } = new();
    }
}
