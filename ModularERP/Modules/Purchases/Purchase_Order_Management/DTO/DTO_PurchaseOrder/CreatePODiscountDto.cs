namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder
{
    public class CreatePODiscountDto
    {
        public string DiscountType { get; set; } = "Percentage";
        public decimal DiscountValue { get; set; }
        public string? Description { get; set; }
    }
}
