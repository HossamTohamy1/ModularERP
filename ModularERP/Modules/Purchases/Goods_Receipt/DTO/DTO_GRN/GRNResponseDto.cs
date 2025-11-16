namespace ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN
{
    public class GRNResponseDto
    {
        public Guid Id { get; set; }
        public string GRNNumber { get; set; } = string.Empty;
        public Guid PurchaseOrderId { get; set; }
        public string PurchaseOrderNumber { get; set; } = string.Empty;
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public DateTime ReceiptDate { get; set; }
        public string? ReceivedBy { get; set; }
        public string? Notes { get; set; }
        public Guid CompanyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedById { get; set; }
        public string? CreatedByName { get; set; }

        // ✨ NEW: PO Status Tracking
        public POStatusInfo PurchaseOrderStatus { get; set; } = new();

        // ✨ NEW: Enhanced Line Items
        public List<GRNLineItemResponseDto> LineItems { get; set; } = new();

        // ✨ NEW: Inventory Impact Summary
        public List<InventoryImpactDto> InventoryImpact { get; set; } = new();

        // ✨ NEW: Next Actions
        public GRNNextActionsDto NextActions { get; set; } = new();
    }
}