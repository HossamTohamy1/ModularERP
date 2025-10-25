using ModularERP.Common.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Models
{
    public class GRNLineItem : BaseEntity
    {
        public Guid GRNId { get; set; }
        public Guid POLineItemId { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public string? Notes { get; set; }

        // Navigation Properties
        public virtual GoodsReceiptNote GRN { get; set; } = null!;
        public virtual POLineItem POLineItem { get; set; } = null!;
    }
}

