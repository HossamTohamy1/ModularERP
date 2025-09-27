using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Modules.Finance.Features.Currencies.Models;
using ModularERP.Modules.Finance.Features.GlAccounts.Models;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;
using ModularERP.Modules.Finance.Features.Treasuries.Models;
using ModularERP.Modules.Finance.Features.Treasuries.Queries;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.Treasuries.Handlers
{
    public class GetTreasuryByIdHandler : IRequestHandler<GetTreasuryByIdQuery, ResponseViewModel<TreasuryDto>>
    {
        private readonly IGeneralRepository<Treasury> _treasuryRepository;
        private readonly IGeneralRepository<Company> _companyRepository;
        private readonly IGeneralRepository<Currency> _currencyRepository;
        private readonly IGeneralRepository<GlAccount> _glAccountRepository; // Add GL Account repository
        private readonly IMapper _mapper;

        public GetTreasuryByIdHandler(
            IGeneralRepository<Treasury> treasuryRepository,
            IGeneralRepository<Company> companyRepository,
            IGeneralRepository<Currency> currencyRepository,
            IGeneralRepository<GlAccount> glAccountRepository, // Add GL Account repository
            IMapper mapper)
        {
            _treasuryRepository = treasuryRepository;
            _companyRepository = companyRepository;
            _currencyRepository = currencyRepository;
            _glAccountRepository = glAccountRepository; // Initialize GL Account repository
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<TreasuryDto>> Handle(GetTreasuryByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var treasury = await _treasuryRepository
                    .Get(t => t.Id == request.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (treasury == null)
                {
                    return ResponseViewModel<TreasuryDto>.Error(
                        "Treasury not found",
                        FinanceErrorCode.TreasuryNotFound);
                }

                var dto = _mapper.Map<TreasuryDto>(treasury);

                // Get company name
                var company = await _companyRepository
                    .Get(c => c.Id == treasury.CompanyId)
                    .FirstOrDefaultAsync(cancellationToken);
                dto.CompanyName = company?.Name;

                // Get currency name
                var currency = await _currencyRepository
                    .Get(c => c.Code == treasury.CurrencyCode)
                    .FirstOrDefaultAsync(cancellationToken);
                dto.CurrencyName = currency?.Code;

                // Get journal account name
                if (treasury.JournalAccountId.HasValue)
                {
                    var journalAccount = await _glAccountRepository
                        .Get(g => g.Id == treasury.JournalAccountId.Value)
                        .FirstOrDefaultAsync(cancellationToken);
                    dto.JournalAccountName = journalAccount?.Name;
                }

                // Get vouchers count
                dto.VouchersCount = await _treasuryRepository
                    .Get(t => t.Id == treasury.Id)
                    .SelectMany(t => t.Vouchers)
                    .CountAsync(cancellationToken);

                return ResponseViewModel<TreasuryDto>.Success(dto, "Treasury retrieved successfully");
            }
            catch (Exception ex)
            {
                return ResponseViewModel<TreasuryDto>.Error(
                    "An error occurred while retrieving the treasury",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}