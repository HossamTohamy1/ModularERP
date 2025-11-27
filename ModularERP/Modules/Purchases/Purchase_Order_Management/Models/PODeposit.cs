using ModularERP.Common.Models;
using ModularERP.Modules.Purchases.Payment.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Models
{
    public class PODeposit : BaseEntity
    {
        public Guid PurchaseOrderId { get; set; }

        public decimal Amount { get; set; }

        public decimal? Percentage { get; set; }

        [Required]
        public Guid PaymentMethodId { get; set; }

        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }

        public bool AlreadyPaid { get; set; }

        public DateTime? PaymentDate { get; set; }

        public string? Notes { get; set; }

        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
        public virtual PaymentMethod PaymentMethod { get; set; } = null!; 
    }
}