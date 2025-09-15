using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;

namespace ModularERP.Modules.Finance.Features.Treasuries.Queries
{
    public class GetTreasuryStatisticsQuery
        : IRequest<ResponseViewModel<TreasuryStatisticsDto>>
    {
        public Guid? CompanyId { get; set; }
    }

}
