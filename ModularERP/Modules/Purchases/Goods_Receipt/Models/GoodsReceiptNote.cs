using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Models
{
    public class GoodsReceiptNote : BaseEntity
    {
        public string GRNNumber { get; set; } = string.Empty;
        public Guid PurchaseOrderId { get; set; }
        public Guid WarehouseId { get; set; }
        public Guid CompanyId { get; set; }
        public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;
        public string? ReceivedBy { get; set; }
        public string? Notes { get; set; }

        // Navigation Properties
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual Company Company { get; set; } = null!;
        public virtual ApplicationUser? CreatedByUser { get; set; }
        public virtual ICollection<GRNLineItem> LineItems { get; set; } = new List<GRNLineItem>();
    }
}