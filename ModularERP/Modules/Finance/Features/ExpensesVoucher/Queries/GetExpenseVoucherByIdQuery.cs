using MediatR;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO;

namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.Queries
{
    public record GetExpenseVoucherByIdQuery(Guid Id) : IRequest<ExpenseVoucherResponseDto?>;

}
