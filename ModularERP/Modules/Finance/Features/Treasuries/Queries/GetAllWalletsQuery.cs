using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;

namespace ModularERP.Modules.Finance.Features.Treasuries.Queries
{
    public class GetAllWalletsQuery : IRequest<ResponseViewModel<List<WalletPermissionDto>>>
    {
        public string? WalletType { get; set; } // Optional filter: Treasury or BankAccount
        public Guid? CompanyId { get; set; }
    }
}
