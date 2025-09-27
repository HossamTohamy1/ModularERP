using ModularERP.Common.Enum.Finance_Enum;

namespace ModularERP.Modules.Finance.Features.Treasuries.DTO
{
    public class UpdateTreasuryDto
    {

        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public TreasuryStatus Status { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string DepositAcl { get; set; } = "{}";
        public string WithdrawAcl { get; set; } = "{}";
        public Guid? JournalAccountId { get; set; }  // FK to GLAccount

    }
}

