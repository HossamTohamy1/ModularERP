using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;

namespace ModularERP.Modules.Finance.Features.Treasuries.Queries
{
    public class GetUserWalletsQuery : IRequest<ResponseViewModel<List<WalletPermissionDto>>>
    {
        public Guid UserId { get; set; }
        public string? WalletType { get; set; } // Optional filter: Treasury or BankAccount
    }
}
