using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.Commands
{
    public record DeleteExpenseVoucherCommand(Guid Id, Guid UserId)
         : IRequest<ResponseViewModel<string>>;
}
