using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Modules.Finance.Features.Currencies.Models;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;
using ModularERP.Modules.Finance.Features.Treasuries.Models;
using ModularERP.Modules.Finance.Features.Treasuries.Queries;
using ModularERP.SharedKernel.Interfaces;

namespace ModularERP.Modules.Finance.Features.Treasuries.Handlers
{
    public class GetTreasuriesByCompanyHandler
        : IRequestHandler<GetTreasuriesByCompanyQuery, ResponseViewModel<IEnumerable<TreasuryListDto>>>
    {
        private readonly IGeneralRepository<Treasury> _treasuryRepository;
        private readonly IGeneralRepository<Company> _companyRepository;
        private readonly IGeneralRepository<Currency> _currencyRepository;
        private readonly IMapper _mapper;

        public GetTreasuriesByCompanyHandler(
            IGeneralRepository<Treasury> treasuryRepository,
            IGeneralRepository<Company> companyRepository,
            IGeneralRepository<Currency> currencyRepository,
            IMapper mapper)
        {
            _treasuryRepository = treasuryRepository;
            _companyRepository = companyRepository;
            _currencyRepository = currencyRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<IEnumerable<TreasuryListDto>>> Handle(GetTreasuriesByCompanyQuery request, CancellationToken cancellationToken)
        {
            var query = _treasuryRepository.GetAll()
                                           .Where(t => t.CompanyId == request.CompanyId);

            var treasuries = await query
                .OrderBy(t => t.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var treasuryDtos = new List<TreasuryListDto>();

            foreach (var treasury in treasuries)
            {
                var dto = _mapper.Map<TreasuryListDto>(treasury);

                var company = await _companyRepository.Get(c => c.Id == treasury.CompanyId).FirstOrDefaultAsync(cancellationToken);
                dto.CompanyName = company?.Name;

                var currency = await _currencyRepository.Get(c => c.Code == treasury.CurrencyCode).FirstOrDefaultAsync(cancellationToken);
                dto.CurrencyName = currency?.Code;

                dto.VouchersCount = await _treasuryRepository.Get(t => t.Id == treasury.Id)
                                                              .SelectMany(t => t.Vouchers)
                                                              .CountAsync(cancellationToken);

                treasuryDtos.Add(dto);
            }

            return ResponseViewModel<IEnumerable<TreasuryListDto>>.Success(treasuryDtos, "Treasuries retrieved successfully");
        }
    }
}
