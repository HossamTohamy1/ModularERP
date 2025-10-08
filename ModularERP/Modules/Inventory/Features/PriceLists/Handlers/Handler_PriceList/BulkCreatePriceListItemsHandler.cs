using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceList;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceList;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handler_PriceList
{
    public class BulkCreatePriceListItemsHandler : IRequestHandler<BulkCreatePriceListItemsCommand, ResponseViewModel<List<PriceListItemDto>>>
    {
        private readonly IGeneralRepository<PriceListItem> _repository;
        private readonly IGeneralRepository<PriceList> _priceListRepository;
        private readonly IMapper _mapper;

        public BulkCreatePriceListItemsHandler(
            IGeneralRepository<PriceListItem> repository,
            IGeneralRepository<PriceList> priceListRepository,
            IMapper mapper)
        {
            _repository = repository;
            _priceListRepository = priceListRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<List<PriceListItemDto>>> Handle(
            BulkCreatePriceListItemsCommand request,
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

            // Get existing items to check for duplicates
            var existingItems = await _repository
                .Get(x => x.PriceListId == request.PriceListId)
                .Select(x => new { x.ProductId, x.ServiceId })
                .ToListAsync(cancellationToken);

            var entities = new List<PriceListItem>();

            foreach (var itemDto in request.BulkItems.Items)
            {
                // Check for duplicates
                var isDuplicate = existingItems.Any(x =>
                    (itemDto.ProductId.HasValue && x.ProductId == itemDto.ProductId) ||
                    (itemDto.ServiceId.HasValue && x.ServiceId == itemDto.ServiceId));

                if (isDuplicate)
                {
                    continue; // Skip duplicates
                }

                var entity = _mapper.Map<PriceListItem>(itemDto);
                entity.PriceListId = request.PriceListId;
                entity.FinalPrice = CalculateFinalPrice(entity);
                entities.Add(entity);
            }

            if (entities.Count == 0)
            {
                throw new BusinessLogicException(
                    "No valid items to add. All items already exist in the price list.",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            await _repository.AddRangeAsync(entities);
            await _repository.SaveChanges();

            var itemIds = entities.Select(x => x.Id).ToList();
            var results = await _repository
                .Get(x => itemIds.Contains(x.Id))
                .ProjectTo<PriceListItemDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return ResponseViewModel<List<PriceListItemDto>>.Success(
                results,
                $"{results.Count} items added to price list successfully");
        }

        private decimal? CalculateFinalPrice(PriceListItem item)
        {
            if (!item.ListPrice.HasValue) return null;

            var finalPrice = item.ListPrice.Value;

            if (item.DiscountValue.HasValue && item.DiscountValue.Value > 0)
            {
                if (item.DiscountType == "%")
                {
                    finalPrice -= (finalPrice * item.DiscountValue.Value / 100);
                }
                else if (item.DiscountType == "Fixed")
                {
                    finalPrice -= item.DiscountValue.Value;
                }
            }

            return finalPrice > 0 ? finalPrice : 0;
        }
    }

}
