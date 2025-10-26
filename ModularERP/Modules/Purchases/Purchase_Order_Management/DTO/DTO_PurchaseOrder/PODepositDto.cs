namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder
{
    public class PODepositDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public decimal? Percentage { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public bool AlreadyPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Notes { get; set; }
    }
}
