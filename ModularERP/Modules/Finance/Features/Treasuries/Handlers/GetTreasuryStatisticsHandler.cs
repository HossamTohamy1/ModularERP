using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;
using ModularERP.Modules.Finance.Features.Treasuries.Models;
using ModularERP.Modules.Finance.Features.Treasuries.Queries;
using ModularERP.SharedKernel.Interfaces;

namespace ModularERP.Modules.Finance.Features.Treasuries.Handlers
{
    public class GetTreasuryStatisticsHandler
        : IRequestHandler<GetTreasuryStatisticsQuery, ResponseViewModel<TreasuryStatisticsDto>>
    {
        private readonly IGeneralRepository<Treasury> _treasuryRepository;

        public GetTreasuryStatisticsHandler(IGeneralRepository<Treasury> treasuryRepository)
        {
            _treasuryRepository = treasuryRepository;
        }

        public async Task<ResponseViewModel<TreasuryStatisticsDto>> Handle(GetTreasuryStatisticsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _treasuryRepository.GetAll();

                if (request.CompanyId.HasValue)
                {
                    query = query.Where(t => t.CompanyId == request.CompanyId.Value);
                }

                var treasuries = await query.ToListAsync(cancellationToken);

                var statistics = new TreasuryStatisticsDto
                {
                    TotalCount = treasuries.Count,
                    ActiveCount = treasuries.Count(t => t.Status == TreasuryStatus.Active),
                    InactiveCount = treasuries.Count(t => t.Status == TreasuryStatus.Inactive),
                    TotalVouchers = treasuries.Sum(t => t.Vouchers?.Count ?? 0),
                    CurrencyDistribution = treasuries
                        .GroupBy(t => t.CurrencyCode)
                        .Select(g => new CurrencyDistributionDto
                        {
                            Currency = g.Key,
                            Count = g.Count()
                        })
                        .ToList()
                };

                return ResponseViewModel<TreasuryStatisticsDto>.Success(statistics, "Treasury statistics retrieved successfully");
            }
            catch (Exception)
            {
                return ResponseViewModel<TreasuryStatisticsDto>.Error(
                    "An error occurred while retrieving treasury statistics",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
