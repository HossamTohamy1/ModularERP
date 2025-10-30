namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POStatus
{
    public class POLineItemPrintDto
    {
        public int LineNumber { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
    }
}
