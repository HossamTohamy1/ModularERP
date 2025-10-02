using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.TaxManagement.DTO;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;
using ModularERP.Modules.Inventory.Features.TaxManagement.Qeuries;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Handlers
{
    public class GetAllTaxComponentsHandler : IRequestHandler<GetAllTaxComponentsQuery, ResponseViewModel<List<TaxComponentDto>>>
    {
        private readonly IGeneralRepository<TaxComponent> _repository;
        private readonly IMapper _mapper;

        public GetAllTaxComponentsHandler(IGeneralRepository<TaxComponent> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<List<TaxComponentDto>>> Handle(GetAllTaxComponentsQuery request, CancellationToken cancellationToken)
        {
            var components = await _repository.GetAll().ToListAsync(cancellationToken);
            var componentDtos = _mapper.Map<List<TaxComponentDto>>(components);

            return ResponseViewModel<List<TaxComponentDto>>.Success(componentDtos, "Tax components retrieved successfully");
        }
    }
}
