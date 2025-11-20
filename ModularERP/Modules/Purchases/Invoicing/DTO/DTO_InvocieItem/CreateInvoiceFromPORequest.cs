namespace ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem
{
    public class CreateInvoiceFromPORequest
    {
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public decimal DepositApplied { get; set; }
        public string? Notes { get; set; }
    }
}
