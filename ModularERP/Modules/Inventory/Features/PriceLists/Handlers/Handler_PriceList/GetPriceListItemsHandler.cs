using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceList;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuries_PriceList;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handler_PriceList
{
    public class GetPriceListItemsHandler : IRequestHandler<GetPriceListItemsQuery, ResponseViewModel<List<PriceListItemListDto>>>
    {
        private readonly IGeneralRepository<PriceListItem> _repository;
        private readonly IGeneralRepository<PriceList> _priceListRepository;
        private readonly IMapper _mapper;

        public GetPriceListItemsHandler(
            IGeneralRepository<PriceListItem> repository,
            IGeneralRepository<PriceList> priceListRepository,
            IMapper mapper)
        {
            _repository = repository;
            _priceListRepository = priceListRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<List<PriceListItemListDto>>> Handle(
            GetPriceListItemsQuery request,
            CancellationToken cancellationToken)
        {
            // Verify price list exists
            var priceListExists = await _priceListRepository.AnyAsync(pl => pl.Id == request.PriceListId);
            if (!priceListExists)
            {
                throw new NotFoundException(
                    $"Price list with ID {request.PriceListId} not found",
                    FinanceErrorCode.NotFound);
            }

            var query = _repository.Get(x => x.PriceListId == request.PriceListId);

            // Filter by active status
            if (!request.IncludeInactive)
            {
                query = query.Where(x => x.IsActive);
            }

            // Filter by validity date
            if (request.AsOfDate.HasValue)
            {
                var asOfDate = request.AsOfDate.Value;
                query = query.Where(x =>
                    (!x.ValidFrom.HasValue || x.ValidFrom <= asOfDate) &&
                    (!x.ValidTo.HasValue || x.ValidTo >= asOfDate));
            }

            var items = await query
                .OrderBy(x => x.CreatedAt)
                .ProjectTo<PriceListItemListDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return ResponseViewModel<List<PriceListItemListDto>>.Success(
                items,
                $"Retrieved {items.Count} items from price list");
        }
    }
}
