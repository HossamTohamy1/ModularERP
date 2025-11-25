namespace ModularERP.Modules.Purchases.KPIs.DTO
{
    public class TopSupplierDto
    {
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalPurchaseAmount { get; set; }
        public int OrderCount { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal OnTimeDeliveryRate { get; set; }
    }
}
