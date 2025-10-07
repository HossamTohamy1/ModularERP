using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Barcode;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Qeuires_Barcode;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handler_Barcode
{
    public class GetBarcodeSettingsHandler : IRequestHandler<GetBarcodeSettingsQuery, ResponseViewModel<BarcodeSettingsDto>>
    {
        private readonly IGeneralRepository<BarcodeSettings> _repository;
        private readonly IMapper _mapper;

        public GetBarcodeSettingsHandler(IGeneralRepository<BarcodeSettings> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<BarcodeSettingsDto>> Handle(GetBarcodeSettingsQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetAll()
                .Where(x => x.IsDefault)
                .ProjectTo<BarcodeSettingsDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (result == null)
            {
                throw new NotFoundException(
                    "No default barcode settings found",
                    FinanceErrorCode.NotFound);
            }

            return ResponseViewModel<BarcodeSettingsDto>.Success(result, "Barcode settings retrieved successfully");
        }
    }
}
