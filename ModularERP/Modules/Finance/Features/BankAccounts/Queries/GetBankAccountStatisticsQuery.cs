using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.DTO;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Queries
{
    public class GetBankAccountStatisticsQuery : IRequest<ResponseViewModel<BankAccountStatisticsDto>>
    {
        public Guid? CompanyId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public GetBankAccountStatisticsQuery()
        {
        }

        public GetBankAccountStatisticsQuery(Guid? companyId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            CompanyId = companyId;
            FromDate = fromDate;
            ToDate = toDate;
        }
    }
}
