namespace ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce
{
    public class SupplierStatusDto
    {
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; }
        public decimal BalanceAdjusted { get; set; }
        public decimal NewBalance { get; set; }
    }
}
