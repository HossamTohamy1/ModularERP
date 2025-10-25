using ModularERP.Common.Models;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;

namespace ModularERP.Modules.Purchases.Refunds.Models
{
    public class DebitNote : BaseEntity
    {
        public string DebitNoteNumber { get; set; } = string.Empty;
        public Guid RefundId { get; set; }
        public Guid SupplierId { get; set; }
        public DateTime NoteDate { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        public string? Notes { get; set; }

        // Navigation Properties
        public virtual PurchaseRefund Refund { get; set; } = null!;
        public virtual Supplier Supplier { get; set; } = null!;
    }
}