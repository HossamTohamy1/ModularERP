using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Finance.Features.WalletPermissions.Commands
{
    public class DeleteWalletPermissionCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid WalletId { get; set; }
        public string WalletType { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }
}
