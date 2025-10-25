using ModularERP.Common.Models;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Models
{
    public class PODeposit : BaseEntity
    {
        public Guid PurchaseOrderId { get; set; }
        public decimal Amount { get; set; }
        public decimal? Percentage { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // Cash, Bank, Cheque, CreditCard
        public string? ReferenceNumber { get; set; }
        public bool AlreadyPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Notes { get; set; }

        // Navigation Properties
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
    }
}