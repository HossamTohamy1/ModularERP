using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commands_UnitTemplates;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handelrs_UnitTemplates
{
    public class DeleteUnitConversionHandler : IRequestHandler<DeleteUnitConversionCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<UnitConversion> _conversionRepository;

        public DeleteUnitConversionHandler(IGeneralRepository<UnitConversion> conversionRepository)
        {
            _conversionRepository = conversionRepository;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteUnitConversionCommand request, CancellationToken cancellationToken)
        {
            var conversion = await _conversionRepository
                .Get(x => x.Id == request.UnitConversionId &&
                         x.UnitTemplateId == request.UnitTemplateId)
                .FirstOrDefaultAsync(cancellationToken);

            if (conversion == null)
            {
                throw new NotFoundException(
                    $"Unit conversion with ID {request.UnitConversionId} not found",
                    Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);
            }

            await _conversionRepository.Delete(request.UnitConversionId);

            return ResponseViewModel<bool>.Success(
                true,
                "Unit conversion deleted successfully");
        }
    }
}
