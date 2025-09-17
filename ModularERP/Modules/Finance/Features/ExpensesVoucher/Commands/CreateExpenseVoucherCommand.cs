using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO;

namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.Commands
{
    public record CreateExpenseVoucherCommand(CreateExpenseVoucherDto Request, Guid UserId)
        : IRequest<ResponseViewModel<ExpenseVoucherResponseDto>>;
}
