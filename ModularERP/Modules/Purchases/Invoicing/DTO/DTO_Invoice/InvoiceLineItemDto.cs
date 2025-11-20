namespace ModularERP.Modules.Purchases.Invoicing.DTO.DTO_Invoice
{
    public class InvoiceLineItemDto
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid POLineItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
    }
}
