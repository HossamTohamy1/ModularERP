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
    public class CreatePriceListItemHandler : IRequestHandler<CreatePriceListItemCommand, ResponseViewModel<PriceListItemDto>>
    {
        private readonly IGeneralRepository<PriceListItem> _repository;
        private readonly IGeneralRepository<PriceList> _priceListRepository;
        private readonly IMapper _mapper;

        public CreatePriceListItemHandler(
            IGeneralRepository<PriceListItem> repository,
            IGeneralRepository<PriceList> priceListRepository,
            IMapper mapper)
        {
            _repository = repository;
            _priceListRepository = priceListRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<PriceListItemDto>> Handle(
            CreatePriceListItemCommand request,
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

            // Check for duplicate item
            var duplicateExists = await _repository.AnyAsync(x =>
                x.PriceListId == request.PriceListId &&
                ((request.Item.ProductId.HasValue && x.ProductId == request.Item.ProductId) ||
                 (request.Item.ServiceId.HasValue && x.ServiceId == request.Item.ServiceId)));

            if (duplicateExists)
            {
                throw new BusinessLogicException(
                    "This product or service already exists in the price list",
                    "Inventory",
                    FinanceErrorCode.DuplicateEntity);
            }

            var entity = _mapper.Map<PriceListItem>(request.Item);
            entity.PriceListId = request.PriceListId;
            entity.FinalPrice = CalculateFinalPrice(entity);

            await _repository.AddAsync(entity);
            await _repository.SaveChanges();

            var result = await _repository
                .Get(x => x.Id == entity.Id)
                .ProjectTo<PriceListItemDto>(_mapper.ConfigurationProvider)
                .FirstAsync(cancellationToken);

            return ResponseViewModel<PriceListItemDto>.Success(result, "Price list item created successfully");
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
