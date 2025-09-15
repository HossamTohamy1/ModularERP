namespace ModularERP.Modules.Finance.Features.Treasuries.DTO
{
    public class UpdateWalletPermissionDto
    {
        public string DepositAcl { get; set; } = "{}";
        public string WithdrawAcl { get; set; } = "{}";
    }
}
