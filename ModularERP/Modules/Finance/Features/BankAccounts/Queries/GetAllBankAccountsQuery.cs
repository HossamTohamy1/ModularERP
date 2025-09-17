using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.DTO;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Queries
{
    public class GetAllBankAccountsQuery : IRequest<ResponseViewModel<IEnumerable<BankAccountListDto>>>
    {
        public Guid? CompanyId { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;

        public GetAllBankAccountsQuery()
        {
        }

        public GetAllBankAccountsQuery(Guid? companyId = null, string? searchTerm = null, int pageNumber = 1, int pageSize = 10, string? sortBy = null, bool sortDescending = false)
        {
            CompanyId = companyId;
            SearchTerm = searchTerm;
            PageNumber = pageNumber;
            PageSize = pageSize;
            SortBy = sortBy;
            SortDescending = sortDescending;
        }
    }
}
