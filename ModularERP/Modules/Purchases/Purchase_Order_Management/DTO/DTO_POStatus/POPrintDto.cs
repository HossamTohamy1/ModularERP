namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POStatus
{
    public class POPrintDto
    {
        public string PONumber { get; set; } = string.Empty;
        public DateTime PODate { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierAddress { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public string? PaymentTerms { get; set; }
        public List<POLineItemPrintDto> LineItems { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public string? Terms { get; set; }
        public string ReceptionStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string DocumentStatus { get; set; } = string.Empty;

    }
}
