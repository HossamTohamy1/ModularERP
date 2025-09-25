using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Taxs.DTO;

namespace ModularERP.Modules.Finance.Features.Taxs.Queries
{
    public class GetActiveTaxesQuery : IRequest<ResponseViewModel<List<TaxListDto>>>
    {
    }
}
