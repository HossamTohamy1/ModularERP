namespace ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN
{
    public class CreateGRNLineItemDto
    {
        public Guid POLineItemId { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public string? Notes { get; set; }
    }
}
