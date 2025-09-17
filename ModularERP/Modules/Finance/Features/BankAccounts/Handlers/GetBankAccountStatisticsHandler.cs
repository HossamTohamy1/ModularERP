using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.DTO;
using ModularERP.Modules.Finance.Features.BankAccounts.Queries;
using ModularERP.Modules.Finance.Features.BankAccounts.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Handlers
{
    public class GetBankAccountStatisticsHandler : IRequestHandler<GetBankAccountStatisticsQuery, ResponseViewModel<BankAccountStatisticsDto>>
    {
        private readonly IGeneralRepository<BankAccount> _bankAccountRepository;

        public GetBankAccountStatisticsHandler(IGeneralRepository<BankAccount> bankAccountRepository)
        {
            _bankAccountRepository = bankAccountRepository;
        }

        public async Task<ResponseViewModel<BankAccountStatisticsDto>> Handle(GetBankAccountStatisticsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _bankAccountRepository.GetAll();

                if (request.CompanyId.HasValue)
                {
                    query = query.Where(ba => ba.CompanyId == request.CompanyId.Value);
                }

                var bankAccounts = await query.ToListAsync(cancellationToken);

                var statistics = new BankAccountStatisticsDto
                {
                    TotalCount = bankAccounts.Count,
                    ActiveCount = bankAccounts.Count(ba => ba.Status == BankAccountStatus.Active),
                    InactiveCount = bankAccounts.Count(ba => ba.Status == BankAccountStatus.Inactive),
                    TotalVouchers = bankAccounts.Sum(ba => ba.Vouchers?.Count ?? 0),
                    CurrencyDistribution = bankAccounts
                        .GroupBy(ba => ba.CurrencyCode)
                        .Select(g => new BankCurrencyDistributionDto
                        {
                            Currency = g.Key,
                            Count = g.Count()
                        })
                        .ToList(),
                    BankDistribution = bankAccounts
                        .GroupBy(ba => ba.BankName)
                        .Select(g => new BankDistributionDto
                        {
                            BankName = g.Key,
                            Count = g.Count()
                        })
                        .ToList()
                };

                return ResponseViewModel<BankAccountStatisticsDto>.Success(statistics, "Bank account statistics retrieved successfully");
            }
            catch (Exception)
            {
                return ResponseViewModel<BankAccountStatisticsDto>.Error(
                    "An error occurred while retrieving bank account statistics",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
