namespace ModularERP.Modules.Finance.Features.Treasuries.DTO
{
    public class TreasuryStatisticsDto
    {
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public int TotalVouchers { get; set; }
        public List<CurrencyDistributionDto> CurrencyDistribution { get; set; }
    }
}
