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
    public class BulkUpdatePriceListItemsHandler : IRequestHandler<BulkUpdatePriceListItemsCommand, ResponseViewModel<List<PriceListItemDto>>>
    {
        private readonly IGeneralRepository<PriceListItem> _repository;
        private readonly IMapper _mapper;

        public BulkUpdatePriceListItemsHandler(
            IGeneralRepository<PriceListItem> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<List<PriceListItemDto>>> Handle(
            BulkUpdatePriceListItemsCommand request,
            CancellationToken cancellationToken)
        {
            var itemIds = request.BulkItems.Items.Select(x => x.ItemId).ToList();

            var entities = await _repository
                .Get(x => x.PriceListId == request.PriceListId && itemIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            if (entities.Count == 0)
            {
                throw new NotFoundException(
                    "No matching items found in the price list",
                    FinanceErrorCode.NotFound);
            }

            var updatedEntities = new List<PriceListItem>();

            foreach (var updateDto in request.BulkItems.Items)
            {
                var entity = entities.FirstOrDefault(x => x.Id == updateDto.ItemId);
                if (entity == null) continue;

                if (updateDto.BasePrice.HasValue)
                    entity.BasePrice = updateDto.BasePrice.Value;

                if (updateDto.ListPrice.HasValue)
                    entity.ListPrice = updateDto.ListPrice.Value;

                if (updateDto.DiscountValue.HasValue)
                    entity.DiscountValue = updateDto.DiscountValue.Value;

                if (!string.IsNullOrEmpty(updateDto.DiscountType))
                    entity.DiscountType = updateDto.DiscountType;

                if (updateDto.TaxProfileId.HasValue)
                    entity.TaxProfileId = updateDto.TaxProfileId.Value;

                if (updateDto.ValidFrom.HasValue)
                    entity.ValidFrom = updateDto.ValidFrom.Value;

                if (updateDto.ValidTo.HasValue)
                    entity.ValidTo = updateDto.ValidTo.Value;

                entity.FinalPrice = CalculateFinalPrice(entity);
                entity.UpdatedAt = DateTime.UtcNow;

                updatedEntities.Add(entity);
            }

            await _repository.SaveChanges();

            var updatedIds = updatedEntities.Select(x => x.Id).ToList();
            var results = await _repository
                .Get(x => updatedIds.Contains(x.Id))
                .ProjectTo<PriceListItemDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return ResponseViewModel<List<PriceListItemDto>>.Success(
                results,
                $"{results.Count} items updated successfully");
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