using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.TaxManagement.DTO;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;
using ModularERP.Modules.Inventory.Features.TaxManagement.Qeuries;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Handlers
{
    public class GetAllTaxProfilesHandler : IRequestHandler<GetAllTaxProfilesQuery, ResponseViewModel<List<TaxProfileDto>>>
    {
        private readonly IGeneralRepository<TaxProfile> _repository;
        private readonly IMapper _mapper;

        public GetAllTaxProfilesHandler(IGeneralRepository<TaxProfile> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<List<TaxProfileDto>>> Handle(GetAllTaxProfilesQuery request, CancellationToken cancellationToken)
        {
            var profiles = await _repository.GetAll().ToListAsync(cancellationToken);
            var profileDtos = _mapper.Map<List<TaxProfileDto>>(profiles);

            return ResponseViewModel<List<TaxProfileDto>>.Success(profileDtos, "Tax profiles retrieved successfully");
        }
    }

}