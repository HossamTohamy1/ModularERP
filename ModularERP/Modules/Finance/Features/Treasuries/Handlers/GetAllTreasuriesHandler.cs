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
    public class GetAllTreasuriesHandler : IRequestHandler<GetAllTreasuriesQuery, ResponseViewModel<IEnumerable<TreasuryListDto>>>
    {
        private readonly IGeneralRepository<Treasury> _treasuryRepository;
        private readonly IGeneralRepository<Company> _companyRepository;
        private readonly IGeneralRepository<Currency> _currencyRepository;
        private readonly IGeneralRepository<GlAccount> _glAccountRepository; // Add this
        private readonly IMapper _mapper;

        public GetAllTreasuriesHandler(
            IGeneralRepository<Treasury> treasuryRepository,
            IGeneralRepository<Company> companyRepository,
            IGeneralRepository<Currency> currencyRepository,
            IGeneralRepository<GlAccount> glAccountRepository, // Add this
            IMapper mapper)
        {
            _treasuryRepository = treasuryRepository;
            _companyRepository = companyRepository;
            _currencyRepository = currencyRepository;
            _glAccountRepository = glAccountRepository; // Add this
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<IEnumerable<TreasuryListDto>>> Handle(GetAllTreasuriesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _treasuryRepository.GetAll();

                if (request.CompanyId.HasValue)
                {
                    query = query.Where(t => t.CompanyId == request.CompanyId.Value);
                }

                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    query = query.Where(t => t.Name.Contains(request.SearchTerm) ||
                                            (t.Description != null && t.Description.Contains(request.SearchTerm)));
                }

                var treasuries = await query
                    .OrderBy(t => t.Name)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                var treasuryDtos = new List<TreasuryListDto>();

                foreach (var treasury in treasuries)
                {
                    var dto = _mapper.Map<TreasuryListDto>(treasury);

                    var company = await _companyRepository
                        .Get(c => c.Id == treasury.CompanyId)
                        .FirstOrDefaultAsync(cancellationToken);
                    dto.CompanyName = company?.Name;

                    var currency = await _currencyRepository
                        .Get(c => c.Code == treasury.CurrencyCode)
                        .FirstOrDefaultAsync(cancellationToken);
                    dto.CurrencyName = currency?.Code;

                    // Add Journal Account Name logic
                    if (treasury.JournalAccountId.HasValue)
                    {
                        var journalAccount = await _glAccountRepository
                            .Get(g => g.Id == treasury.JournalAccountId.Value)
                            .FirstOrDefaultAsync(cancellationToken);
                        dto.JournalAccountName = journalAccount?.Name;
                    }

                    dto.VouchersCount = await _treasuryRepository
                        .Get(t => t.Id == treasury.Id)
                        .SelectMany(t => t.Vouchers)
                        .CountAsync(cancellationToken);

                    treasuryDtos.Add(dto);
                }

                return ResponseViewModel<IEnumerable<TreasuryListDto>>.Success(
                    treasuryDtos,
                    $"Retrieved {treasuryDtos.Count} treasuries successfully");
            }
            catch (Exception ex)
            {
                return ResponseViewModel<IEnumerable<TreasuryListDto>>.Error(
                    "An error occurred while retrieving treasuries",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
