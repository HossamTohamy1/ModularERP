using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.TaxManagement.Commends;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Handlers
{
    public class AddTaxComponentToProfileHandler : IRequestHandler<AddTaxComponentToProfileCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<TaxProfile> _profileRepository;
        private readonly IGeneralRepository<TaxComponent> _componentRepository;
        private readonly IJoinTableRepository<TaxProfileComponent> _profileComponentRepository;

        public AddTaxComponentToProfileHandler(
            IGeneralRepository<TaxProfile> profileRepository,
            IGeneralRepository<TaxComponent> componentRepository,
            IJoinTableRepository<TaxProfileComponent> profileComponentRepository)
        {
            _profileRepository = profileRepository;
            _componentRepository = componentRepository;
            _profileComponentRepository = profileComponentRepository;
        }

        public async Task<ResponseViewModel<bool>> Handle(AddTaxComponentToProfileCommand request, CancellationToken cancellationToken)
        {
            var profile = await _profileRepository.GetByID(request.TaxProfileId);
            if (profile == null)
            {
                throw new NotFoundException("Tax profile not found", FinanceErrorCode.NotFound);
            }

            var component = await _componentRepository.GetByID(request.TaxComponentId);
            if (component == null)
            {
                throw new NotFoundException("Tax component not found", FinanceErrorCode.NotFound);
            }

            // Check if already exists
            var exists = await _profileComponentRepository.AnyAsync(pc =>
                pc.TaxProfileId == request.TaxProfileId &&
                pc.TaxComponentId == request.TaxComponentId);

            if (exists)
            {
                throw new BusinessLogicException("Tax component already added to this profile", "TaxManagement");
            }

            var profileComponent = new TaxProfileComponent
            {
                TaxProfileId = request.TaxProfileId,
                TaxComponentId = request.TaxComponentId,
                Priority = request.Priority
            };

            await _profileComponentRepository.AddAsync(profileComponent);
            await _profileComponentRepository.SaveChangesAsync();

            return ResponseViewModel<bool>.Success(true, "Tax component added to profile successfully");
        }
    }
}

