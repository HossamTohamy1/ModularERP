using ModularERP.Common.Enum.Purchases_Enum;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder
{
    public class PODepositDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public decimal? Percentage { get; set; }
        // ✅ PaymentMethod details من الـ Entity
        public Guid PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; } = string.Empty;
        public string PaymentMethodCode { get; set; } = string.Empty;
        public bool PaymentMethodRequiresReference { get; set; }
        public string? ReferenceNumber { get; set; }
        public bool AlreadyPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Notes { get; set; }
    }
}
