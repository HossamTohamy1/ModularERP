using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.WalletPermissions.DTO;

namespace ModularERP.Modules.Finance.Features.WalletPermissions.Commands
{
    public class UpdateWalletPermissionCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid WalletId { get; set; }
        public string WalletType { get; set; } = string.Empty;
        public UpdateWalletPermissionDto Permissions { get; set; } = null!;
    }
}
