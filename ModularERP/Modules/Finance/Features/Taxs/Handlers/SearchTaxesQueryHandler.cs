using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Taxs.DTO;
using ModularERP.Modules.Finance.Features.Taxs.Models;
using ModularERP.Modules.Finance.Features.Taxs.Queries;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.Taxs.Handlers
{
    public class SearchTaxesQueryHandler : IRequestHandler<SearchTaxesQuery, ResponseViewModel<List<TaxListDto>>>
    {
        private readonly IGeneralRepository<Tax> _taxRepository;
        private readonly IMapper _mapper;

        public SearchTaxesQueryHandler(IGeneralRepository<Tax> taxRepository, IMapper mapper)
        {
            _taxRepository = taxRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<List<TaxListDto>>> Handle(SearchTaxesQuery request, CancellationToken cancellationToken)
        {
            var query = _taxRepository.GetAll();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(t =>
                    t.Name.Contains(request.SearchTerm) ||
                    t.Code.Contains(request.SearchTerm));
            }

            var taxes = await query
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);

            var response = _mapper.Map<List<TaxListDto>>(taxes);
            return ResponseViewModel<List<TaxListDto>>.Success(response);
        }
    }
}
