using ModularERP.Common.Enum.Finance_Enum;

namespace ModularERP.Modules.Finance.Features.BankAccounts.DTO
{
    public class BankAccountDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public BankAccountStatus Status { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string DepositAcl { get; set; } = string.Empty;
        public string WithdrawAcl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        // Related data
        public string? CompanyName { get; set; }
        public string? CurrencyName { get; set; }
        public int VouchersCount { get; set; }
    }
}
