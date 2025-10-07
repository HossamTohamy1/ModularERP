using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Barcode;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Barcode;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handler_Barcode
{
    public class UpdateBarcodeSettingsHandler : IRequestHandler<UpdateBarcodeSettingsCommand, ResponseViewModel<BarcodeSettingsDto>>
    {
        private readonly IGeneralRepository<BarcodeSettings> _repository;
        private readonly IMapper _mapper;

        public UpdateBarcodeSettingsHandler(IGeneralRepository<BarcodeSettings> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<BarcodeSettingsDto>> Handle(UpdateBarcodeSettingsCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIDWithTracking(request.Id);

            if (entity == null)
            {
                throw new NotFoundException(
                    $"Barcode setting with ID {request.Id} not found",
                    FinanceErrorCode.NotFound);
            }

            // Check if trying to set as default when another default exists
            if (request.IsDefault && !entity.IsDefault)
            {
                var hasOtherDefaultSetting = await _repository.GetAll()
                    .AnyAsync(x => x.IsDefault && x.Id != request.Id, cancellationToken);

                if (hasOtherDefaultSetting)
                {
                    throw new BusinessLogicException(
                        "Another default barcode setting already exists. Please unset it first.",
                        "Inventory",
                        FinanceErrorCode.BusinessLogicError);
                }
            }

            _mapper.Map(request, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            await _repository.SaveChanges();

            var result = await _repository.GetAll()
                .Where(x => x.Id == entity.Id)
                .ProjectTo<BarcodeSettingsDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            return ResponseViewModel<BarcodeSettingsDto>.Success(result, "Barcode settings updated successfully");
        }
    }
}