namespace ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem
{
    public class AddInvoiceItemRequest
    {
        public Guid POLineItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
    }
}
