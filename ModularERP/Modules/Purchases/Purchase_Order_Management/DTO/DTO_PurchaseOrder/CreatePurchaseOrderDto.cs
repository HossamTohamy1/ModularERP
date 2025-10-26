namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder
{
    public class CreatePurchaseOrderDto
    {
        public Guid Id { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public DateTime PODate { get; set; }
        public string? PaymentTerms { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal AdjustmentAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal AmountDue { get; set; }
        public string ReceptionStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string DocumentStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
