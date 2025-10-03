using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ItemGroup;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.Products.Qeuries.Queries_ItemGroup;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Products.Handlers.Handlers_ItemGroup
{
    public class GetAllItemGroupsQueryHandler : IRequestHandler<GetAllItemGroupsQuery, ResponseViewModel<List<ItemGroupDto>>>
    {
        private readonly IGeneralRepository<ItemGroup> _repository;
        private readonly IMapper _mapper;

        public GetAllItemGroupsQueryHandler(IGeneralRepository<ItemGroup> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<List<ItemGroupDto>>> Handle(GetAllItemGroupsQuery request, CancellationToken cancellationToken)
        {
            var itemGroups = await _repository.GetAll().ToListAsync(cancellationToken);

            // Manual mapping for related entities
            var categoryIds = itemGroups.Where(x => x.CategoryId.HasValue).Select(x => x.CategoryId!.Value).Distinct().ToList();
            var brandIds = itemGroups.Where(x => x.BrandId.HasValue).Select(x => x.BrandId!.Value).Distinct().ToList();

            var result = _mapper.Map<List<ItemGroupDto>>(itemGroups);

            return ResponseViewModel<List<ItemGroupDto>>.Success(result, "Item groups retrieved successfully");
        }
    }
}
