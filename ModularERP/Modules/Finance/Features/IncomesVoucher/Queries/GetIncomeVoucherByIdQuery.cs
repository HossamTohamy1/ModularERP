using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.IncomesVoucher.DTO;

namespace ModularERP.Modules.Finance.Features.IncomesVoucher.Queries
{
    public record GetIncomeVoucherByIdQuery(Guid Id) : IRequest<ResponseViewModel<IncomeVoucherResponseDto?>>;

}
