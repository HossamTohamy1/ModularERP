using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.GlAccounts.DTO;

namespace ModularERP.Modules.Finance.Features.GlAccounts.Queries
{
    public class GetAllGlAccountsQuery : IRequest<ResponseViewModel<List<GlAccountResponseDto>>>
    {
        public Guid? CompanyId { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public GetAllGlAccountsQuery(Guid? companyId = null, string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
        {
            CompanyId = companyId;
            SearchTerm = searchTerm;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
