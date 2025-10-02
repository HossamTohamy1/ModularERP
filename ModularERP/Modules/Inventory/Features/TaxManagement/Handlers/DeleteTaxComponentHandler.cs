using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.TaxManagement.Commends;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Handlers
{
    public class DeleteTaxComponentHandler : IRequestHandler<DeleteTaxComponentCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<TaxComponent> _componentRepository;
        private readonly IJoinTableRepository<TaxProfileComponent> _profileComponentRepository;

        public DeleteTaxComponentHandler(
            IGeneralRepository<TaxComponent> componentRepository,
            IJoinTableRepository<TaxProfileComponent> profileComponentRepository)
        {
            _componentRepository = componentRepository;
            _profileComponentRepository = profileComponentRepository;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteTaxComponentCommand request, CancellationToken cancellationToken)
        {
            var component = await _componentRepository.GetByID(request.Id);
            if (component == null)
            {
                throw new NotFoundException("Tax component not found", FinanceErrorCode.NotFound);
            }

            // Check if component is used in any tax profile
            var isUsed = await _profileComponentRepository.AnyAsync(pc => pc.TaxComponentId == request.Id);
            if (isUsed)
            {
                throw new BusinessLogicException("Cannot delete tax component because it is used in one or more tax profiles", "TaxManagement");
            }

            await _componentRepository.Delete(request.Id);

            return ResponseViewModel<bool>.Success(true, "Tax component deleted successfully");
        }
    }
}
