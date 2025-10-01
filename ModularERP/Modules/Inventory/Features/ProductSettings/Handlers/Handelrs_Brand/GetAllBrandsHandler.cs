using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Brand;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Modules.Inventory.Features.ProductSettings.Qeuries.Qeuires_Brand;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handelrs_Brand
{
    public class GetAllBrandsHandler : IRequestHandler<GetAllBrandsQuery, ResponseViewModel<List<BrandDto>>>
    {
        private readonly IGeneralRepository<Brand> _repository;
        private readonly IMapper _mapper;

        public GetAllBrandsHandler(
            IGeneralRepository<Brand> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<List<BrandDto>>> Handle(GetAllBrandsQuery request, CancellationToken cancellationToken)
        {
            var brands = await _repository.GetAll()
                .OrderBy(b => b.Name)
                .ToListAsync(cancellationToken);

            var brandDtos = _mapper.Map<List<BrandDto>>(brands);
            return ResponseViewModel<List<BrandDto>>.Success(brandDtos, "Brands retrieved successfully");
        }
    }
}