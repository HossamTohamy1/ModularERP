namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POItme
{
    public class POLineItemListDto
    {
        public Guid Id { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public string? ProductName { get; set; }
        public string? ServiceName { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public decimal RemainingQuantity { get; set; }
        public bool IsActive { get; set; }
    }
}
