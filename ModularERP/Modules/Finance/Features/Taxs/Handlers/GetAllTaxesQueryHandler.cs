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
    public class GetAllTaxesQueryHandler : IRequestHandler<GetAllTaxesQuery, ResponseViewModel<List<TaxListDto>>>
    {
        private readonly IGeneralRepository<Tax> _taxRepository;
        private readonly IMapper _mapper;

        public GetAllTaxesQueryHandler(IGeneralRepository<Tax> taxRepository, IMapper mapper)
        {
            _taxRepository = taxRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<List<TaxListDto>>> Handle(GetAllTaxesQuery request, CancellationToken cancellationToken)
        {
            var taxes = await _taxRepository.GetAll()
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);

            var response = _mapper.Map<List<TaxListDto>>(taxes);
            return ResponseViewModel<List<TaxListDto>>.Success(response);
        }
    }
}
