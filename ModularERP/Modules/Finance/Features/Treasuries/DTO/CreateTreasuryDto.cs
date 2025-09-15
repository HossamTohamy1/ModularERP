using ModularERP.Common.Enum.Finance_Enum;

namespace ModularERP.Modules.Finance.Features.Treasuries.DTO
{
    public class CreateTreasuryDto
    {
        public Guid CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public TreasuryStatus Status { get; set; } = TreasuryStatus.Active;
        public string CurrencyCode { get; set; } = "EGP";
        public string? Description { get; set; }
        public string DepositAcl { get; set; } = "{}";
        public string WithdrawAcl { get; set; } = "{}";
    }
}
