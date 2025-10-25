using ModularERP.Common.Models;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Models
{
    public class POAttachment : BaseEntity
    {
        public Guid PurchaseOrderId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public Guid UploadedBy { get; set; }

        // Navigation Properties
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
        public virtual ApplicationUser UploadedByUser { get; set; } = null!;
    }
}
