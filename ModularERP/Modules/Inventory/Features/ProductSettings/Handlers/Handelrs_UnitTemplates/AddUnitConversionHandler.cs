using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commands_UnitTemplates;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_UnitTemplates;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handelrs_UnitTemplates
{
    public class AddUnitConversionHandler : IRequestHandler<AddUnitConversionCommand, ResponseViewModel<UnitConversionDto>>
    {
        private readonly IGeneralRepository<UnitTemplate> _templateRepository;
        private readonly IGeneralRepository<UnitConversion> _conversionRepository;
        private readonly IMapper _mapper;

        public AddUnitConversionHandler(
            IGeneralRepository<UnitTemplate> templateRepository,
            IGeneralRepository<UnitConversion> conversionRepository,
            IMapper mapper)
        {
            _templateRepository = templateRepository;
            _conversionRepository = conversionRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<UnitConversionDto>> Handle(AddUnitConversionCommand request, CancellationToken cancellationToken)
        {
            var template = await _templateRepository.GetByID(request.UnitTemplateId);

            if (template == null)
            {
                throw new NotFoundException(
                    $"Unit template with ID {request.UnitTemplateId} not found",
                    Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);
            }

            var existingConversion = await _conversionRepository
                .Get(x => x.UnitTemplateId == request.UnitTemplateId &&
                         x.ShortName == request.Data.ShortName)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingConversion != null)
            {
                throw new BusinessLogicException(
                    $"Unit conversion with short name '{request.Data.ShortName}' already exists",
                    "Inventory");
            }

            var conversion = _mapper.Map<UnitConversion>(request.Data);
            conversion.Id = Guid.NewGuid();
            conversion.UnitTemplateId = request.UnitTemplateId;
            conversion.CompanyId = Guid.Parse("90137c92-382d-404c-bade-e5fefb1e8dbf");


            await _conversionRepository.AddAsync(conversion);
            await _conversionRepository.SaveChanges();

            var conversionDto = _mapper.Map<UnitConversionDto>(conversion);

            return ResponseViewModel<UnitConversionDto>.Success(
                conversionDto,
                "Unit conversion added successfully");
        }
    }
}
