using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Taxs.Commands;
using ModularERP.Modules.Finance.Features.Taxs.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.Taxs.Handlers
{
    public class ToggleTaxStatusCommandHandler : IRequestHandler<ToggleTaxStatusCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Tax> _taxRepository;

        public ToggleTaxStatusCommandHandler(IGeneralRepository<Tax> taxRepository)
        {
            _taxRepository = taxRepository;
        }

        public async Task<ResponseViewModel<bool>> Handle(ToggleTaxStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Business Logic: Get existing tax
                var tax = await _taxRepository.GetByIDWithTracking(request.TaxId);

                if (tax == null)
                {
                    throw new NotFoundException(
                        $"Tax with ID '{request.TaxId}' not found",
                        FinanceErrorCode.NotFound);
                }

                // Business Logic: Toggle status
                tax.IsActive = !tax.IsActive;
                tax.UpdatedAt = DateTime.UtcNow;

                await _taxRepository.SaveChanges();

                var status = tax.IsActive ? "activated" : "deactivated";
                return ResponseViewModel<bool>.Success(true, $"Tax '{tax.Name}' {status} successfully");
            }
            catch (Exception ex) when (!(ex is NotFoundException))
            {
                throw new BusinessLogicException(
                    $"Error toggling tax status: {ex.Message}",
                    "Finance",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}
