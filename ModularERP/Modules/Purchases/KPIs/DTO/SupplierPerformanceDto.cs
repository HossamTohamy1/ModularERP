namespace ModularERP.Modules.Purchases.KPIs.DTO
{
    public class SupplierPerformanceDto
    {
        public List<TopSupplierDto> TopSuppliers { get; set; } = new();
        public decimal AverageSupplierRating { get; set; }
        public int TotalActiveSuppliers { get; set; }
        public int SuppliersWithOutstandingBalance { get; set; }
    }
}
