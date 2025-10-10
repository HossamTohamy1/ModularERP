using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_BulkDiscount;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_BulkDiscount;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_BulkDiscount
{
    public class GetBulkDiscountsByPriceListQueryHandler : IRequestHandler<GetBulkDiscountsByPriceListQuery, ResponseViewModel<List<BulkDiscountDto>>>
    {
        private readonly IGeneralRepository<BulkDiscount> _repository;
        private readonly IMapper _mapper;

        public GetBulkDiscountsByPriceListQueryHandler(
            IGeneralRepository<BulkDiscount> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<List<BulkDiscountDto>>> Handle(GetBulkDiscountsByPriceListQuery request, CancellationToken cancellationToken)
        {
            var discounts = await _repository
                .GetAll()
                .Where(bd => bd.PriceListId == request.PriceListId && !bd.IsDeleted)
                .ProjectTo<BulkDiscountDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return ResponseViewModel<List<BulkDiscountDto>>.Success(discounts, "Bulk discounts retrieved successfully");
        }
    }

}
