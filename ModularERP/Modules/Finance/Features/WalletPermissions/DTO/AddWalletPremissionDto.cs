using Microsoft.AspNetCore.Mvc;

namespace ModularERP.Modules.Finance.Features.WalletPermissions.DTO
{
    public class AddWalletPremissionDto
    {
        [FromRoute]
        public Guid Id { get; set; }

        [FromQuery]
        public string Type { get; set; } = string.Empty;

        [FromBody]
        public Guid UserId { get; set; }

        [FromBody]
        public bool CanDeposit { get; set; }

        [FromBody]
        public bool CanWithdraw { get; set; }
    }
}
