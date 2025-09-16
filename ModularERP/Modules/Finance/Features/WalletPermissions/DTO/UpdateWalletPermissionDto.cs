namespace ModularERP.Modules.Finance.Features.WalletPermissions.DTO
{
    public class UpdateWalletPermissionDto
    {
        public Guid UserId { get; set; }
        public bool CanDeposit { get; set; }
        public bool CanWithdraw { get; set; }

    }
}
