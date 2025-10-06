using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_UnitTemplates;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Qeuires_UnitTemplates;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handelrs_UnitTemplates
{
    public class GetAllUnitTemplatesHandler : IRequestHandler<GetAllUnitTemplatesQuery, ResponseViewModel<List<UnitTemplateListDto>>>
    {
        private readonly IGeneralRepository<UnitTemplate> _repository;
        private readonly IMapper _mapper;

        public GetAllUnitTemplatesHandler(IGeneralRepository<UnitTemplate> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<List<UnitTemplateListDto>>> Handle(GetAllUnitTemplatesQuery request, CancellationToken cancellationToken)
        {
            var templatesDto = await _repository.GetAll()
                .OrderBy(x => x.Name)
                .ProjectTo<UnitTemplateListDto>(_mapper.ConfigurationProvider) 
                .ToListAsync(cancellationToken);

            return ResponseViewModel<List<UnitTemplateListDto>>.Success(
                templatesDto,
                "Unit templates retrieved successfully");
        }
    }
}