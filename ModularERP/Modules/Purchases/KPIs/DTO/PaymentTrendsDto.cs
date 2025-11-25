namespace ModularERP.Modules.Purchases.KPIs.DTO
{
    public class PaymentTrendsDto
    {
        public decimal TotalPaid { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal PaymentCompletionRate { get; set; }
        public decimal AveragePaymentDelay { get; set; }
        public List<PaymentStatusBreakdownDto> StatusBreakdown { get; set; } = new();
        public List<MonthlyPaymentDto> MonthlyTrends { get; set; } = new();
    }

    public class PaymentStatusBreakdownDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class MonthlyPaymentDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal TotalPaid { get; set; }
        public decimal TotalDue { get; set; }
    }
}
