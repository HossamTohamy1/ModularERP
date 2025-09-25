using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Finance.Features.IncomesVoucher.Commands
{
    public record DeleteIncomeVoucherCommand(Guid Id, Guid UserId) : IRequest<ResponseViewModel<bool>>;

}
