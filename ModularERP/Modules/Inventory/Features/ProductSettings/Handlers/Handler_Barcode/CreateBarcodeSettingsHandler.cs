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
    public class CreateBarcodeSettingsHandler : IRequestHandler<CreateBarcodeSettingsCommand, ResponseViewModel<BarcodeSettingsDto>>
    {
        private readonly IGeneralRepository<BarcodeSettings> _repository;
        private readonly IMapper _mapper;

        public CreateBarcodeSettingsHandler(IGeneralRepository<BarcodeSettings> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<BarcodeSettingsDto>> Handle(CreateBarcodeSettingsCommand request, CancellationToken cancellationToken)
        {
            // Check if default setting already exists when trying to create a new default
            if (request.IsDefault)
            {
                var hasDefaultSetting = await _repository.GetAll()
                    .AnyAsync(x => x.IsDefault, cancellationToken);

                if (hasDefaultSetting)
                {
                    throw new BusinessLogicException(
                        "A default barcode setting already exists. Please update the existing one or set IsDefault to false.",
                        "Inventory",
                        FinanceErrorCode.BusinessLogicError);
                }
            }

            var entity = _mapper.Map<BarcodeSettings>(request);
            entity.CompanyId= Guid.Parse("90137c92-382d-404c-bade-e5fefb1e8dbf");

            await _repository.AddAsync(entity);
            await _repository.SaveChanges();

            var result = await _repository.GetAll()
                .Where(x => x.Id == entity.Id)
                .ProjectTo<BarcodeSettingsDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            return ResponseViewModel<BarcodeSettingsDto>.Success(result, "Barcode settings created successfully");
        }
    }
}

