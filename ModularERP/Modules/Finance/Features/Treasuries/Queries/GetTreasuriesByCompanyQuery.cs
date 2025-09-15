using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;

namespace ModularERP.Modules.Finance.Features.Treasuries.Queries
{
    public class GetTreasuriesByCompanyQuery
        : IRequest<ResponseViewModel<IEnumerable<TreasuryListDto>>>
    {
        public Guid CompanyId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
