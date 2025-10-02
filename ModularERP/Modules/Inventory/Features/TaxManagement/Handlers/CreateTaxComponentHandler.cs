using AutoMapper;
using MediatR;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.TaxManagement.Commends;
using ModularERP.Modules.Inventory.Features.TaxManagement.DTO;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Handlers
{
    public class CreateTaxComponentHandler : IRequestHandler<CreateTaxComponentCommand, ResponseViewModel<TaxComponentDto>>
    {
        private readonly IGeneralRepository<TaxComponent> _repository;
        private readonly IMapper _mapper;

        public CreateTaxComponentHandler(IGeneralRepository<TaxComponent> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<TaxComponentDto>> Handle(CreateTaxComponentCommand request, CancellationToken cancellationToken)
        {
            // Check if name already exists
            var exists = await _repository.AnyAsync(tc => tc.Name == request.Name);
            if (exists)
            {
                throw new BusinessLogicException("Tax component with this name already exists", "TaxManagement");
            }

            var component = _mapper.Map<TaxComponent>(request);
            await _repository.AddAsync(component);
            await _repository.SaveChanges();

            var componentDto = _mapper.Map<TaxComponentDto>(component);
            return ResponseViewModel<TaxComponentDto>.Success(componentDto, "Tax component created successfully");
        }
    }

}
