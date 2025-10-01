using AutoMapper;
using FluentValidation;
using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Brand;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Brand;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Shared.Interfaces;
using AppValidationException = ModularERP.Common.Exceptions.ValidationException;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handelrs_Brand
{
    public class CreateBrandHandler : IRequestHandler<CreateBrandCommand, ResponseViewModel<BrandDto>>
    {
        private readonly IGeneralRepository<Brand> _repository;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateBrandCommand> _validator;

        public CreateBrandHandler(
            IGeneralRepository<Brand> repository,
            IMapper mapper,
            IValidator<CreateBrandCommand> validator)
        {
            _repository = repository;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<ResponseViewModel<BrandDto>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                throw new AppValidationException("Validation failed for creating brand", errors, "Inventory");
            }

            var brand = _mapper.Map<Brand>(request.Dto);
            brand.Id = Guid.NewGuid();
            brand.CreatedAt = DateTime.UtcNow;

            await _repository.AddAsync(brand);
            await _repository.SaveChanges();

            var brandDto = _mapper.Map<BrandDto>(brand);
            return ResponseViewModel<BrandDto>.Success(brandDto, "Brand created successfully");
        }
    }
}