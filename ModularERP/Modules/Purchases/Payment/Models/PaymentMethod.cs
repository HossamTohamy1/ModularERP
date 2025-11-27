using ModularERP.Common.Models;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Purchases.Payment.Models
{
    public class PaymentMethod : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // Cash, Bank Transfer, Cheque, Credit Card

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty; // CASH, BANK, CHQ, CC

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool RequiresReference { get; set; } = false; // true for cheque/bank transfer

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<SupplierPayment> SupplierPayments { get; set; } = new List<SupplierPayment>();
        public virtual ICollection<PODeposit> PODeposits { get; set; } = new List<PODeposit>();
    }
}