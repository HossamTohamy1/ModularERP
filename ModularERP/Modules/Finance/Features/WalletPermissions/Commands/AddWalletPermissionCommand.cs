using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Finance.Features.WalletPermissions.Commands
{
    public class AddWalletPermissionCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid WalletId { get; set; }
        public string WalletType { get; set; } = string.Empty; // Treasury or BankAccount
        public Guid UserId { get; set; }
        public bool CanDeposit { get; set; }
        public bool CanWithdraw { get; set; }
    }
}
