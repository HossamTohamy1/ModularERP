namespace ModularERP.Modules.Finance.Features.WalletPermissions.DTO
{
    public class UserWalletDto
    {
        public Guid UserId { get; set; }
        public List<WalletPermissionDto> Wallets { get; set; } = new List<WalletPermissionDto>();
    }
}
