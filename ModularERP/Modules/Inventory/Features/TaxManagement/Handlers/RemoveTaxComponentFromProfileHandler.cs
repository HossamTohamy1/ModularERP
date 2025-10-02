using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.TaxManagement.Commends;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Handlers
{
    public class RemoveTaxComponentFromProfileHandler : IRequestHandler<RemoveTaxComponentFromProfileCommand, ResponseViewModel<bool>>
    {
        private readonly IJoinTableRepository<TaxProfileComponent> _repository;

        public RemoveTaxComponentFromProfileHandler(IJoinTableRepository<TaxProfileComponent> repository)
        {
            _repository = repository;
        }

        public async Task<ResponseViewModel<bool>> Handle(RemoveTaxComponentFromProfileCommand request, CancellationToken cancellationToken)
        {
            var profileComponent = await _repository
                .Get(pc => pc.TaxProfileId == request.TaxProfileId && pc.TaxComponentId == request.TaxComponentId)
                .FirstOrDefaultAsync(cancellationToken);

            if (profileComponent == null)
            {
                throw new NotFoundException("Tax component not found in this profile", FinanceErrorCode.NotFound);
            }

            // Soft delete the component
            await _repository.Delete(profileComponent);

            return ResponseViewModel<bool>.Success(true, "Tax component removed from profile successfully");
        }
    }
}
