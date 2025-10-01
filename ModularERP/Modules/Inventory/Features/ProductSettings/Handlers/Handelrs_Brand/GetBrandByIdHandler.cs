using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Brand;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Qeuires_Brand;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handelrs_Brand
{
    public class GetBrandByIdHandler : IRequestHandler<GetBrandByIdQuery, ResponseViewModel<BrandDto>>
    {
        private readonly IGeneralRepository<Brand> _repository;
        private readonly IMapper _mapper;

        public GetBrandByIdHandler(
            IGeneralRepository<Brand> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<BrandDto>> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
        {
            var brand = await _repository.GetByID(request.Id);
            if (brand == null)
            {
                throw new NotFoundException("Brand not found", FinanceErrorCode.NotFound);
            }

            var brandDto = _mapper.Map<BrandDto>(brand);
            return ResponseViewModel<BrandDto>.Success(brandDto, "Brand retrieved successfully");
        }
    }
}
