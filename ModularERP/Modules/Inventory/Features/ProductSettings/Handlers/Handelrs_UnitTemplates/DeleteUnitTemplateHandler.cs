using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commands_UnitTemplates;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handelrs_UnitTemplates
{
    public class DeleteUnitTemplateHandler : IRequestHandler<DeleteUnitTemplateCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<UnitTemplate> _repository;
        private readonly IGeneralRepository<UnitConversion> _conversionRepository;

        public DeleteUnitTemplateHandler(
            IGeneralRepository<UnitTemplate> repository,
            IGeneralRepository<UnitConversion> conversionRepository)
        {
            _repository = repository;
            _conversionRepository = conversionRepository;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteUnitTemplateCommand request, CancellationToken cancellationToken)
        {
            var template = await _repository.GetByID(request.Id);

            if (template == null)
            {
                throw new NotFoundException(
                    $"Unit template with ID {request.Id} not found",
                    Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);
            }

            var hasConversions = await _conversionRepository
                .Get(x => x.UnitTemplateId == request.Id)
                .AnyAsync(cancellationToken);

            if (hasConversions)
            {
                throw new BusinessLogicException(
                    "Cannot delete unit template that has conversions. Delete conversions first.",
                    "Inventory");
            }

            await _repository.Delete(request.Id);

            return ResponseViewModel<bool>.Success(
                true,
                "Unit template deleted successfully");
        }
    }
}
