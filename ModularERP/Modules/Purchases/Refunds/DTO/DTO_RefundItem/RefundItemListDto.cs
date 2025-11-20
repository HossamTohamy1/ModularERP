namespace ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundItem
{
    public class RefundItemListDto
    {
        public Guid Id { get; set; }
        public Guid GRNLineItemId { get; set; }
        public decimal ReturnQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public string ProductName { get; set; } = string.Empty;
    }
}
