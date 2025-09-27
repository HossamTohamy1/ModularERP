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
    public class GetAllBankAccountsHandler : IRequestHandler<GetAllBankAccountsQuery, ResponseViewModel<IEnumerable<BankAccountListDto>>>
    {
        private readonly IGeneralRepository<BankAccount> _bankAccountRepository;
        private readonly IGeneralRepository<Company> _companyRepository;
        private readonly IGeneralRepository<Currency> _currencyRepository;
        private readonly IGeneralRepository<GlAccount> _glAccountRepository; // Add this
        private readonly IMapper _mapper;

        public GetAllBankAccountsHandler(
            IGeneralRepository<BankAccount> bankAccountRepository,
            IGeneralRepository<Company> companyRepository,
            IGeneralRepository<Currency> currencyRepository,
            IGeneralRepository<GlAccount> glAccountRepository, // Add this
            IMapper mapper)
        {
            _bankAccountRepository = bankAccountRepository;
            _companyRepository = companyRepository;
            _currencyRepository = currencyRepository;
            _glAccountRepository = glAccountRepository; // Add this
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<IEnumerable<BankAccountListDto>>> Handle(GetAllBankAccountsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _bankAccountRepository.GetAll();

                if (request.CompanyId.HasValue)
                {
                    query = query.Where(ba => ba.CompanyId == request.CompanyId.Value);
                }

                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    query = query.Where(ba => ba.Name.Contains(request.SearchTerm) ||
                                             ba.BankName.Contains(request.SearchTerm) ||
                                             ba.AccountNumber.Contains(request.SearchTerm) ||
                                             (ba.Description != null && ba.Description.Contains(request.SearchTerm)));
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

                    dto.VouchersCount = await _bankAccountRepository
                        .Get(ba => ba.Id == bankAccount.Id)
                        .SelectMany(ba => ba.Vouchers)
                        .CountAsync(cancellationToken);

                    bankAccountDtos.Add(dto);
                }

                return ResponseViewModel<IEnumerable<BankAccountListDto>>.Success(
                    bankAccountDtos,
                    $"Retrieved {bankAccountDtos.Count} bank accounts successfully");
            }
            catch (Exception)
            {
                return ResponseViewModel<IEnumerable<BankAccountListDto>>.Error(
                    "An error occurred while retrieving bank accounts",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
