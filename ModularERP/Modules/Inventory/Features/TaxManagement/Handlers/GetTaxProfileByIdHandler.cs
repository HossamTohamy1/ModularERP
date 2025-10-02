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
    public class GetTaxProfileByIdHandler : IRequestHandler<GetTaxProfileByIdQuery, ResponseViewModel<TaxProfileDetailDto>>
    {
        private readonly IGeneralRepository<TaxProfile> _profileRepository;
        private readonly IJoinTableRepository<TaxProfileComponent> _componentRepository;
        private readonly IGeneralRepository<TaxComponent> _taxComponentRepository;
        private readonly IMapper _mapper;

        public GetTaxProfileByIdHandler(
            IGeneralRepository<TaxProfile> profileRepository,
            IJoinTableRepository<TaxProfileComponent> componentRepository,
            IGeneralRepository<TaxComponent> taxComponentRepository,
            IMapper mapper)
        {
            _profileRepository = profileRepository;
            _componentRepository = componentRepository;
            _taxComponentRepository = taxComponentRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<TaxProfileDetailDto>> Handle(GetTaxProfileByIdQuery request, CancellationToken cancellationToken)
        {
            var profile = await _profileRepository.GetByID(request.Id);
            if (profile == null)
            {
                throw new NotFoundException("Tax profile not found", FinanceErrorCode.NotFound);
            }

            var profileDto = _mapper.Map<TaxProfileDetailDto>(profile);

            // Get profile components
            var profileComponents = await _componentRepository
                .Get(pc => pc.TaxProfileId == request.Id)
                .ToListAsync(cancellationToken);

            // Get tax component IDs
            var componentIds = profileComponents.Select(pc => pc.TaxComponentId).ToList();

            // Get tax components
            var taxComponents = await _taxComponentRepository
                .Get(tc => componentIds.Contains(tc.Id))
                .ToListAsync(cancellationToken);

            // Map components with details
            profileDto.Components = profileComponents.Select(pc =>
            {
                var taxComponent = taxComponents.FirstOrDefault(tc => tc.Id == pc.TaxComponentId);
                return new TaxProfileComponentDto
                {
                    TaxComponentId = pc.TaxComponentId,
                    ComponentName = taxComponent?.Name ?? "",
                    RateType = taxComponent?.RateType.ToString() ?? "",
                    RateValue = taxComponent?.RateValue ?? 0,
                    IncludedType = taxComponent?.IncludedType.ToString() ?? "",
                    AppliesOn = taxComponent?.AppliesOn.ToString() ?? "",
                    Priority = pc.Priority
                };
            }).ToList();

            return ResponseViewModel<TaxProfileDetailDto>.Success(profileDto, "Tax profile retrieved successfully");
        }
    }
}
