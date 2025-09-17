using ModularERP.Modules.Finance.Features.Treasuries.DTO;

namespace ModularERP.Modules.Finance.Features.BankAccounts.DTO
{
    public class BankAccountStatisticsDto
    {
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public int TotalVouchers { get; set; }
        public List<BankCurrencyDistributionDto> CurrencyDistribution { get; set; } = new();
        public List<BankDistributionDto> BankDistribution { get; set; } = new();
    }
}
