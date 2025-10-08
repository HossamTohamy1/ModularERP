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
    public class UpdatePriceListItemHandler : IRequestHandler<UpdatePriceListItemCommand, ResponseViewModel<PriceListItemDto>>
    {
        private readonly IGeneralRepository<PriceListItem> _repository;
        private readonly IMapper _mapper;

        public UpdatePriceListItemHandler(
            IGeneralRepository<PriceListItem> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<PriceListItemDto>> Handle(
            UpdatePriceListItemCommand request,
            CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByID(request.ItemId);

            if (entity == null || entity.PriceListId != request.PriceListId)
            {
                throw new NotFoundException(
                    $"Price list item with ID {request.ItemId} not found in price list {request.PriceListId}",
                    FinanceErrorCode.NotFound);
            }

            _mapper.Map(request.Item, entity);
            entity.FinalPrice = CalculateFinalPrice(entity);
            entity.UpdatedAt = DateTime.UtcNow;

            await _repository.Update(entity);

            var result = await _repository
                .Get(x => x.Id == entity.Id)
                .ProjectTo<PriceListItemDto>(_mapper.ConfigurationProvider)
                .FirstAsync(cancellationToken);

            return ResponseViewModel<PriceListItemDto>.Success(result, "Price list item updated successfully");
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
