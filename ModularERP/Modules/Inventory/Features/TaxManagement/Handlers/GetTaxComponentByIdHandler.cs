using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.TaxManagement.DTO;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;
using ModularERP.Modules.Inventory.Features.TaxManagement.Qeuries;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Handlers
{
    public class GetTaxComponentByIdHandler : IRequestHandler<GetTaxComponentByIdQuery, ResponseViewModel<TaxComponentDto>>
    {
        private readonly IGeneralRepository<TaxComponent> _repository;
        private readonly IMapper _mapper;

        public GetTaxComponentByIdHandler(IGeneralRepository<TaxComponent> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<TaxComponentDto>> Handle(GetTaxComponentByIdQuery request, CancellationToken cancellationToken)
        {
            var component = await _repository.GetByID(request.Id);
            if (component == null)
            {
                throw new NotFoundException("Tax component not found", FinanceErrorCode.NotFound);
            }

            var componentDto = _mapper.Map<TaxComponentDto>(component);
            return ResponseViewModel<TaxComponentDto>.Success(componentDto, "Tax component retrieved successfully");
        }
    }

}
