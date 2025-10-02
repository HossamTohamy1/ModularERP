using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.TaxManagement.Commends;
using ModularERP.Modules.Inventory.Features.TaxManagement.DTO;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Handlers
{
    public class CreateTaxProfileHandler : IRequestHandler<CreateTaxProfileCommand, ResponseViewModel<TaxProfileDto>>
    {
        private readonly IGeneralRepository<TaxProfile> _profileRepository;
        private readonly IJoinTableRepository<TaxProfileComponent> _componentRepository;
        private readonly IGeneralRepository<TaxComponent> _taxComponentRepository;
        private readonly IMapper _mapper;

        public CreateTaxProfileHandler(
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

        public async Task<ResponseViewModel<TaxProfileDto>> Handle(CreateTaxProfileCommand request, CancellationToken cancellationToken)
        {
            // Check if name already exists
            var exists = await _profileRepository.AnyAsync(tp => tp.Name == request.Name);
            if (exists)
            {
                throw new BusinessLogicException("Tax profile with this name already exists", "TaxManagement");
            }

            // Validate tax components exist
            if (request.Components.Any())
            {
                var componentIds = request.Components.Select(c => c.TaxComponentId).ToList();
                var existingComponents = await _taxComponentRepository
                    .Get(tc => componentIds.Contains(tc.Id))
                    .ToListAsync(cancellationToken);

                if (existingComponents.Count != componentIds.Count)
                {
                    throw new BusinessLogicException("One or more tax components not found", "TaxManagement");
                }
            }

            var profile = _mapper.Map<TaxProfile>(request);
            await _profileRepository.AddAsync(profile);
            await _profileRepository.SaveChanges();

            // Add components
            if (request.Components.Any())
            {
                var profileComponents = request.Components.Select(c => new TaxProfileComponent
                {
                    TaxProfileId = profile.Id,
                    TaxComponentId = c.TaxComponentId,
                    Priority = c.Priority
                }).ToList();

                await _componentRepository.AddRangeAsync(profileComponents);
                await _componentRepository.SaveChangesAsync();
            }

            var profileDto = _mapper.Map<TaxProfileDto>(profile);
            return ResponseViewModel<TaxProfileDto>.Success(profileDto, "Tax profile created successfully");
        }
    }
}