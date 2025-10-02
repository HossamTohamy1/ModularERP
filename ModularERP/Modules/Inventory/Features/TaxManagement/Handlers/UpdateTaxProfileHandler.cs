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
    public class UpdateTaxProfileHandler : IRequestHandler<UpdateTaxProfileCommand, ResponseViewModel<TaxProfileDto>>
    {
        private readonly IGeneralRepository<TaxProfile> _repository;
        private readonly IMapper _mapper;

        public UpdateTaxProfileHandler(IGeneralRepository<TaxProfile> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<TaxProfileDto>> Handle(UpdateTaxProfileCommand request, CancellationToken cancellationToken)
        {
            var profile = await _repository.GetByID(request.Id);
            if (profile == null)
            {
                throw new NotFoundException("Tax profile not found", FinanceErrorCode.NotFound);
            }

            // Check if new name conflicts with another profile
            var nameExists = await _repository.AnyAsync(tp => tp.Name == request.Name && tp.Id != request.Id);
            if (nameExists)
            {
                throw new BusinessLogicException("Tax profile with this name already exists", "TaxManagement");
            }

            _mapper.Map(request, profile);
            await _repository.Update(profile);

            var profileDto = _mapper.Map<TaxProfileDto>(profile);
            return ResponseViewModel<TaxProfileDto>.Success(profileDto, "Tax profile updated successfully");
        }
    }
}
