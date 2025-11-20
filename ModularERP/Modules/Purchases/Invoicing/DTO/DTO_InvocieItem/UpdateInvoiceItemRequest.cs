namespace ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem
{
    public class UpdateInvoiceItemRequest
    {
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
    }
}
