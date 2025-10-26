namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder
{
    public class POAdjustmentDto
    {
        public Guid Id { get; set; }
        public string AdjustmentType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }
}
