using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Companys.DTO;

namespace ModularERP.Modules.Finance.Features.Companys.Queries
{
    public class GetCompanyByIdQuery : IRequest<ResponseViewModel<CompanyResponseDto>>
    {
        public Guid Id { get; set; }

        public GetCompanyByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
