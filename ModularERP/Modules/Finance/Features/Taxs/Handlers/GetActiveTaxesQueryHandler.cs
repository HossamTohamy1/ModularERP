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
    public class GetActiveTaxesQueryHandler : IRequestHandler<GetActiveTaxesQuery, ResponseViewModel<List<TaxListDto>>>
    {
        private readonly IGeneralRepository<Tax> _taxRepository;
        private readonly IMapper _mapper;

        public GetActiveTaxesQueryHandler(IGeneralRepository<Tax> taxRepository, IMapper mapper)
        {
            _taxRepository = taxRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<List<TaxListDto>>> Handle(GetActiveTaxesQuery request, CancellationToken cancellationToken)
        {
            var activeTaxes = await _taxRepository
                .Get(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);

            var response = _mapper.Map<List<TaxListDto>>(activeTaxes);
            return ResponseViewModel<List<TaxListDto>>.Success(response);
        }
    }
}
