using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commands_UnitTemplates;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_UnitTemplates;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handelrs_UnitTemplates
{
    public class UpdateUnitTemplateHandler : IRequestHandler<UpdateUnitTemplateCommand, ResponseViewModel<UnitTemplateDto>>
    {
        private readonly IGeneralRepository<UnitTemplate> _repository;
        private readonly IMapper _mapper;
        private readonly FinanceDbContext _context; 

        public UpdateUnitTemplateHandler(
            IGeneralRepository<UnitTemplate> repository,
            IMapper mapper,
            FinanceDbContext context)
        {
            _repository = repository;
            _mapper = mapper;
            _context = context;
        }

        public async Task<ResponseViewModel<UnitTemplateDto>> Handle(UpdateUnitTemplateCommand request, CancellationToken cancellationToken)
        {
            var template = await _repository.GetByID(request.Id);

            if (template == null)
            {
                throw new NotFoundException(
                    $"Unit template with ID {request.Id} not found",
                    Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);
            }

            var duplicateTemplate = await _repository
                .Get(x => x.Name == request.Data.Name && x.Id != request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (duplicateTemplate != null)
            {
                throw new BusinessLogicException(
                    $"Unit template with name '{request.Data.Name}' already exists",
                    "Inventory");
            }

            _mapper.Map(request.Data, template);
            template.UpdatedAt = DateTime.UtcNow;

            await _repository.Update(template);

            var templateDto = await _context.Set<UnitTemplate>()
                .Where(x => x.Id == template.Id)
                .ProjectTo<UnitTemplateDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            return ResponseViewModel<UnitTemplateDto>.Success(
                templateDto!,
                "Unit template updated successfully");
        }
    }
}
