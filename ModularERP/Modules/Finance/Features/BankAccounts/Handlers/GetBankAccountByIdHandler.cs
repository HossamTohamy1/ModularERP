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

namespace ModularERP.Modules.Finance.Features.BankAccounts.Handlers
{
    public class GetBankAccountByIdHandler : IRequestHandler<GetBankAccountByIdQuery, ResponseViewModel<BankAccountDto>>
    {
        private readonly IGeneralRepository<BankAccount> _bankAccountRepository;
        private readonly IGeneralRepository<Company> _companyRepository;
        private readonly IGeneralRepository<Currency> _currencyRepository;
        private readonly IMapper _mapper;

        public GetBankAccountByIdHandler(
            IGeneralRepository<BankAccount> bankAccountRepository,
            IGeneralRepository<Company> companyRepository,
            IGeneralRepository<Currency> currencyRepository,
            IMapper mapper)
        {
            _bankAccountRepository = bankAccountRepository ?? throw new ArgumentNullException(nameof(bankAccountRepository));
            _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
            _currencyRepository = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ResponseViewModel<BankAccountDto>> Handle(GetBankAccountByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.Id == Guid.Empty)
                {
                    return ResponseViewModel<BankAccountDto>.Error(
                        "Bank account ID is required",
                        FinanceErrorCode.InvalidData);
                }

                var bankAccount = await _bankAccountRepository
                    .Get(ba => ba.Id == request.Id && !ba.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (bankAccount == null)
                {
                    return ResponseViewModel<BankAccountDto>.Error(
                        "Bank account not found",
                        FinanceErrorCode.BankAccountNotFound);
                }

                var dto = _mapper.Map<BankAccountDto>(bankAccount);

                // Get related data
                var company = await _companyRepository
                    .Get(c => c.Id == bankAccount.CompanyId)
                    .FirstOrDefaultAsync(cancellationToken);
                dto.CompanyName = company?.Name;

                var currency = await _currencyRepository
                    .Get(c => c.Code == bankAccount.CurrencyCode)
                    .FirstOrDefaultAsync(cancellationToken);
                dto.CurrencyName = currency?.Code;

                dto.VouchersCount = bankAccount.Vouchers?.Count ?? 0;

                return ResponseViewModel<BankAccountDto>.Success(dto, "Bank account retrieved successfully");
            }
            catch (Exception ex)
            {
                return ResponseViewModel<BankAccountDto>.Error(
                    $"An error occurred while retrieving the bank account: {ex.Message}",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
