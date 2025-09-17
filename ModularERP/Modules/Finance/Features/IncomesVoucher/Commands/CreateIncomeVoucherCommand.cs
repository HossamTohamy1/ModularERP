using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.IncomesVoucher.DTO;

namespace ModularERP.Modules.Finance.Features.IncomesVoucher.Commands
{
    public record CreateIncomeVoucherCommand(CreateIncomeVoucherDto Request, Guid UserId)
        : IRequest<ResponseViewModel<IncomeVoucherResponseDto>>;
}
