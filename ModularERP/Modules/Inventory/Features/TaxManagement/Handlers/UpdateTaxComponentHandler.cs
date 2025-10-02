using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.TaxManagement.Commends;
using ModularERP.Modules.Inventory.Features.TaxManagement.DTO;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Handlers
{
    public class UpdateTaxComponentHandler : IRequestHandler<UpdateTaxComponentCommand, ResponseViewModel<TaxComponentDto>>
    {
        private readonly IGeneralRepository<TaxComponent> _repository;
        private readonly IMapper _mapper;

        public UpdateTaxComponentHandler(IGeneralRepository<TaxComponent> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<TaxComponentDto>> Handle(UpdateTaxComponentCommand request, CancellationToken cancellationToken)
        {
            var component = await _repository.GetByID(request.Id);
            if (component == null)
            {
                throw new NotFoundException("Tax component not found", FinanceErrorCode.NotFound);
            }

            // Check if new name conflicts with another component
            var nameExists = await _repository.AnyAsync(tc => tc.Name == request.Name && tc.Id != request.Id);
            if (nameExists)
            {
                throw new BusinessLogicException("Tax component with this name already exists", "TaxManagement");
            }

            _mapper.Map(request, component);
            await _repository.Update(component);

            var componentDto = _mapper.Map<TaxComponentDto>(component);
            return ResponseViewModel<TaxComponentDto>.Success(componentDto, "Tax component updated successfully");
        }
    }

}
