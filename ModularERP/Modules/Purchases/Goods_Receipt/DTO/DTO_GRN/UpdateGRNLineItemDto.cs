namespace ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN
{
    public class UpdateGRNLineItemDto
    {
        public Guid? Id { get; set; }
        public Guid POLineItemId { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public string? Notes { get; set; }
    }
}
