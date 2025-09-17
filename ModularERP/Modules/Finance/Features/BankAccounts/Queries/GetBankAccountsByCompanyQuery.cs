using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.DTO;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Queries
{
    public class GetBankAccountsByCompanyQuery : IRequest<ResponseViewModel<IEnumerable<BankAccountListDto>>>
    {
        public Guid CompanyId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }

        public GetBankAccountsByCompanyQuery()
        {
            CompanyId = Guid.Empty;
        }

        public GetBankAccountsByCompanyQuery(Guid companyId, int pageNumber = 1, int pageSize = 10, string? search = null)
        {
            CompanyId = companyId;
            PageNumber = pageNumber;
            PageSize = pageSize;
            Search = search;
        }
    }
}

