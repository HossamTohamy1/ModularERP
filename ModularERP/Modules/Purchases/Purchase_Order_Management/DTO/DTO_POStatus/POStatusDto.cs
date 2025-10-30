namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POStatus
{
    public class POStatusDto
    {
        public Guid PurchaseOrderId { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string ReceptionStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string DocumentStatus { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal AmountDue { get; set; }
        public decimal ReceivedPercentage { get; set; }
        public decimal PaidPercentage { get; set; }
        public DateTime PODate { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}
