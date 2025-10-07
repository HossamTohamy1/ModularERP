using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Barcode;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handler_Barcode
{
    public class DeleteBarcodeSettingsHandler : IRequestHandler<DeleteBarcodeSettingsCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<BarcodeSettings> _repository;

        public DeleteBarcodeSettingsHandler(IGeneralRepository<BarcodeSettings> repository)
        {
            _repository = repository;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteBarcodeSettingsCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByID(request.Id);

            if (entity == null)
            {
                throw new NotFoundException(
                    $"Barcode setting with ID {request.Id} not found",
                    FinanceErrorCode.NotFound);
            }

            await _repository.Delete(request.Id);

            return ResponseViewModel<bool>.Success(true, "Barcode settings deleted successfully");
        }
    }
}