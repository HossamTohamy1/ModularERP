namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder
{
    public class PurchaseOrderListDto
    {
        public Guid Id { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public DateTime PODate { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal AmountDue { get; set; }
        public string ReceptionStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string DocumentStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int LineItemsCount { get; set; }
    }
}
