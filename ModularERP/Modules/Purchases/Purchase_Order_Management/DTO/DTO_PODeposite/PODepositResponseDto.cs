namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PODeposite
{
    public class PODepositResponseDto
    {
        public Guid Id { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public decimal Amount { get; set; }
        public decimal? Percentage { get; set; }

        // ✅ PaymentMethod details
        public Guid PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; } = string.Empty;
        public string PaymentMethodCode { get; set; } = string.Empty; public string? ReferenceNumber { get; set; }
        public bool AlreadyPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // ✅ Add these important fields:
        public decimal POTotal { get; set; }           // Total PO amount
        public decimal TotalDeposits { get; set; }      // Sum of all deposits
        public decimal RemainingBalance { get; set; }   // PO Total - Total Deposits
        public string PaymentStatus { get; set; }       // Current PO payment status
    }
}
