using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_UnitTemplates;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Qeuires_UnitTemplates;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handelrs_UnitTemplates
{
    public class GetUnitTemplateByIdHandler : IRequestHandler<GetUnitTemplateByIdQuery, ResponseViewModel<UnitTemplateDto>>
    {
        private readonly IGeneralRepository<UnitTemplate> _repository;
        private readonly IGeneralRepository<UnitConversion> _conversionRepository;
        private readonly IMapper _mapper;

        public GetUnitTemplateByIdHandler(
            IGeneralRepository<UnitTemplate> repository,
            IGeneralRepository<UnitConversion> conversionRepository,
            IMapper mapper)
        {
            _repository = repository;
            _conversionRepository = conversionRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<UnitTemplateDto>> Handle(GetUnitTemplateByIdQuery request, CancellationToken cancellationToken)
        {
            var template = await _repository.GetByID(request.Id);

            if (template == null)
            {
                throw new NotFoundException(
                    $"Unit template with ID {request.Id} not found",
                    Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);
            }

            var conversions = await _conversionRepository
                .Get(x => x.UnitTemplateId == request.Id)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync(cancellationToken);

            var templateDto = _mapper.Map<UnitTemplateDto>(template);
            templateDto.UnitConversions = _mapper.Map<List<UnitConversionDto>>(conversions);

            return ResponseViewModel<UnitTemplateDto>.Success(
                templateDto,
                "Unit template retrieved successfully");
        }
    }
}
