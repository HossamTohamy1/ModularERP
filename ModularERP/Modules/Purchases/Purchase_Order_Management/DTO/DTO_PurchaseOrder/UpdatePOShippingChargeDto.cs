namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder
{
    public class UpdatePOShippingChargeDto
    {
        public Guid? Id { get; set; }
        public decimal ShippingFee { get; set; }
        public Guid? TaxProfileId { get; set; }
        public string? Description { get; set; }
    }
}
