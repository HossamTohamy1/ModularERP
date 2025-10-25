using ModularERP.Common.Models;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;

namespace ModularERP.Modules.Purchases.Refunds.Models
{
    public class RefundLineItem : BaseEntity
    {
        public Guid RefundId { get; set; }
        public Guid GRNLineItemId { get; set; }
        public decimal ReturnQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }

        // Navigation Properties
        public virtual PurchaseRefund Refund { get; set; } = null!;
        public virtual GRNLineItem GRNLineItem { get; set; } = null!;
    }
}
