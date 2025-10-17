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
    public class CreateUnitTemplateHandler : IRequestHandler<CreateUnitTemplateCommand, ResponseViewModel<UnitTemplateDto>>
    {
        private readonly IGeneralRepository<UnitTemplate> _repository;
        private readonly IMapper _mapper;

        public CreateUnitTemplateHandler(IGeneralRepository<UnitTemplate> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<UnitTemplateDto>> Handle(CreateUnitTemplateCommand request, CancellationToken cancellationToken)
        {
            var existingTemplate = await _repository
                .Get(x => x.Name == request.Data.Name)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingTemplate != null)
            {
                throw new BusinessLogicException(
                    $"Unit template with name '{request.Data.Name}' already exists",
                    "Inventory");
            }

            var template = _mapper.Map<UnitTemplate>(request.Data);
            template.Id = Guid.NewGuid();
            template.CompanyId = Guid.Parse("90137c92-382d-404c-bade-e5fefb1e8dbf");

            await _repository.AddAsync(template);
            await _repository.SaveChanges();

            var templateDto = _mapper.Map<UnitTemplateDto>(template);

            return ResponseViewModel<UnitTemplateDto>.Success(
                templateDto,
                "Unit template created successfully");
        }
    }
}
