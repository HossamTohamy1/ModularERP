namespace ModularERP.Modules.Finance.Features.WalletPermissions.DTO
{
    public class WalletPermissionDto
    {
        public Guid WalletId { get; set; }
        public string WalletType { get; set; } = string.Empty; // Treasury or BankAccount
        public string WalletName { get; set; } = string.Empty;
        public string DepositAcl { get; set; } = "{}";
        public string WithdrawAcl { get; set; } = "{}";
        public string CurrencyCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
