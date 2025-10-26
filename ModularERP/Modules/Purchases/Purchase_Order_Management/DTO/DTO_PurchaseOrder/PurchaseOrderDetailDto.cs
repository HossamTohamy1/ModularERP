namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder
{
    public class PurchaseOrderDetailDto
    {
        public Guid Id { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
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
        public string? Notes { get; set; }
        public string? Terms { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<POLineItemDto> LineItems { get; set; } = new();
        public List<PODepositDto> Deposits { get; set; } = new();
        public List<POShippingChargeDto> ShippingCharges { get; set; } = new();
        public List<PODiscountDto> Discounts { get; set; } = new();
        public List<POAdjustmentDto> Adjustments { get; set; } = new();
        public List<POAttachmentDto> Attachments { get; set; } = new();
        public List<PONoteDto> PONotes { get; set; } = new();
    }
}
