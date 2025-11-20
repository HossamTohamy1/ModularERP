namespace ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce
{
    public class CreateRefundLineItemDto
    {
        public Guid GRNLineItemId { get; set; }
        public decimal ReturnQuantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
