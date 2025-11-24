using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POStatus;

namespace ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce
{
    public class StatusUpdatesDto
    {
        public POStatusDto PurchaseOrder { get; set; }
        public SupplierStatusDto Supplier { get; set; }
    }
}
