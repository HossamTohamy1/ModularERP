using MediatR;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO;

namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.Commands
{
    public record CreateExpenseVoucherCommand(CreateExpenseVoucherDto Request, Guid UserId)
        : IRequest<ExpenseVoucherResponseDto>;
}
