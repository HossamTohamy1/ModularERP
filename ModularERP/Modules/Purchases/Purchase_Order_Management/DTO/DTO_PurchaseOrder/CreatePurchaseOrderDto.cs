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
        public Guid? PaymentTermId { get; set; }
        public string? PaymentTermName { get; set; }

        public string? Notes { get; set; }
        public string? Terms { get; set; }
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

        // New fields for complete response
        public List<POLineItemDto> LineItems { get; set; } = new();
        public List<PODepositDto> Deposits { get; set; } = new();
        public List<PODiscountDto> Discounts { get; set; } = new();
        public List<POAdjustmentDto> Adjustments { get; set; } = new();
        public List<POShippingChargeDto> ShippingCharges { get; set; } = new();
        public List<string> PONotes { get; set; } = new();
        public int AttachmentCount { get; set; }
    }
}
