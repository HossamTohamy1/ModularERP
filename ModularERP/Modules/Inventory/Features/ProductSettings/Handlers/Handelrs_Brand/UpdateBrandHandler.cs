using AutoMapper;
using FluentValidation;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Brand;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Brand;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Shared.Interfaces;
using AppValidationException = ModularERP.Common.Exceptions.ValidationException;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handelrs_Brand
{
    public class UpdateBrandHandler : IRequestHandler<UpdateBrandCommand, ResponseViewModel<BrandDto>>
    {
        private readonly IGeneralRepository<Brand> _repository;
        private readonly IMapper _mapper;
        private readonly IValidator<UpdateBrandCommand> _validator;

        public UpdateBrandHandler(
            IGeneralRepository<Brand> repository,
            IMapper mapper,
            IValidator<UpdateBrandCommand> validator)
        {
            _repository = repository;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<ResponseViewModel<BrandDto>> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                throw new AppValidationException("Validation failed for updating brand", errors, "Inventory");
            }

            var existingBrand = await _repository.GetByIDWithTracking(request.Dto.Id);
            if (existingBrand == null)
            {
                throw new NotFoundException("Brand not found", FinanceErrorCode.NotFound);
            }

            _mapper.Map(request.Dto, existingBrand);
            existingBrand.UpdatedAt = DateTime.UtcNow;

            await _repository.SaveChanges();

            var brandDto = _mapper.Map<BrandDto>(existingBrand);
            return ResponseViewModel<BrandDto>.Success(brandDto, "Brand updated successfully");
        }
    }
}
