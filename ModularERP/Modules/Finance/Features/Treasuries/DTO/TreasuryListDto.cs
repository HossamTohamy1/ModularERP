using ModularERP.Common.Enum.Finance_Enum;

namespace ModularERP.Modules.Finance.Features.Treasuries.DTO
{
    public class TreasuryListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TreasuryStatus Status { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CompanyName { get; set; }
        public string? CurrencyName { get; set; }
        public int VouchersCount { get; set; }
    }
}
