using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;

namespace ModularERP.Modules.Finance.Features.Treasuries.Queries
{
    public class GetWalletPermissionsQuery : IRequest<ResponseViewModel<WalletPermissionDto>>
    {
        public Guid WalletId { get; set; }
        public string WalletType { get; set; } = string.Empty;
    }
}
