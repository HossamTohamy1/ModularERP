using ModularERP.Common.Enum.Finance_Enum;

namespace ModularERP.Modules.Finance.Features.Treasuries.DTO
{
    public class TreasuryDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public TreasuryStatus Status { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string DepositAcl { get; set; } = string.Empty;
        public string WithdrawAcl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        // Related data
        public string? CompanyName { get; set; }
        public string? CurrencyName { get; set; }
        public Guid? JournalAccountId { get; set; }  // FK to GLAccount
        public string? JournalAccountName { get; set; }

        public int VouchersCount { get; set; }
    }
}
