using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.DTO;
using ModularERP.Modules.Finance.Features.BankAccounts.Queries;
using ModularERP.Modules.Finance.Features.BankAccounts.Models;
using ModularERP.Shared.Interfaces;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Modules.Finance.Features.Currencies.Models;
using ModularERP.Modules.Finance.Features.GlAccounts.Models;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Handlers
{
    public class GetBankAccountsByCompanyHandler : IRequestHandler<GetBankAccountsByCompanyQuery, ResponseViewModel<IEnumerable<BankAccountListDto>>>
    {
        private readonly IGeneralRepository<BankAccount> _bankAccountRepository;
        private readonly IGeneralRepository<Company> _companyRepository;
        private readonly IGeneralRepository<Currency> _currencyRepository;
        private readonly IGeneralRepository<GlAccount> _glAccountRepository; // Add this
        private readonly IMapper _mapper;

        public GetBankAccountsByCompanyHandler(
            IGeneralRepository<BankAccount> bankAccountRepository,
            IGeneralRepository<Company> companyRepository,
            IGeneralRepository<Currency> currencyRepository,
            IGeneralRepository<GlAccount> glAccountRepository, // Add this
            IMapper mapper)
        {
            _bankAccountRepository = bankAccountRepository ?? throw new ArgumentNullException(nameof(bankAccountRepository));
            _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
            _currencyRepository = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));
            _glAccountRepository = glAccountRepository ?? throw new ArgumentNullException(nameof(glAccountRepository)); // Add this
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ResponseViewModel<IEnumerable<BankAccountListDto>>> Handle(GetBankAccountsByCompanyQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.CompanyId == Guid.Empty)
                {
                    return ResponseViewModel<IEnumerable<BankAccountListDto>>.Error(
                        "Company ID is required",
                        FinanceErrorCode.InvalidData);
                }

                var query = _bankAccountRepository.Get(ba => ba.CompanyId == request.CompanyId && !ba.IsDeleted);

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(request.Search))
                {
                    query = query.Where(ba => ba.Name.Contains(request.Search) ||
                                             ba.BankName.Contains(request.Search) ||
                                             ba.AccountNumber.Contains(request.Search) ||
                                             (ba.Description != null && ba.Description.Contains(request.Search)));
                }

                var bankAccounts = await query
                    .OrderBy(ba => ba.BankName)
                    .ThenBy(ba => ba.Name)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                var bankAccountDtos = new List<BankAccountListDto>();

                foreach (var bankAccount in bankAccounts)
                {
                    var dto = _mapper.Map<BankAccountListDto>(bankAccount);

                    var company = await _companyRepository
                        .Get(c => c.Id == bankAccount.CompanyId)
                        .FirstOrDefaultAsync(cancellationToken);
                    dto.CompanyName = company?.Name;

                    var currency = await _currencyRepository
                        .Get(c => c.Code == bankAccount.CurrencyCode)
                        .FirstOrDefaultAsync(cancellationToken);
                    dto.CurrencyName = currency?.Code;

                    // Add Journal Account Name logic
                    if (bankAccount.JournalAccountId.HasValue)
                    {
                        var journalAccount = await _glAccountRepository
                            .Get(g => g.Id == bankAccount.JournalAccountId.Value)
                            .FirstOrDefaultAsync(cancellationToken);
                        dto.JournalAccountName = journalAccount?.Name;
                    }

                    dto.VouchersCount = bankAccount.Vouchers?.Count ?? 0;
                    bankAccountDtos.Add(dto);
                }

                return ResponseViewModel<IEnumerable<BankAccountListDto>>.Success(bankAccountDtos, "Bank accounts retrieved successfully");
            }
            catch (Exception ex)
            {
                return ResponseViewModel<IEnumerable<BankAccountListDto>>.Error(
                    $"An error occurred while retrieving bank accounts: {ex.Message}",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
