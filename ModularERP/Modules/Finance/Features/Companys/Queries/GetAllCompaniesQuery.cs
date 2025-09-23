using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Companys.DTO;

namespace ModularERP.Modules.Finance.Features.Companys.Queries
{
    public class GetAllCompaniesQuery : IRequest<ResponseViewModel<List<CompanyResponseDto>>>
    {
    }
}
