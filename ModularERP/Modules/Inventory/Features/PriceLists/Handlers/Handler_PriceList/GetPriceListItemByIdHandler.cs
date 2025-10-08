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
    public class GetPriceListItemByIdHandler : IRequestHandler<GetPriceListItemByIdQuery, ResponseViewModel<PriceListItemDto>>
    {
        private readonly IGeneralRepository<PriceListItem> _repository;
        private readonly IMapper _mapper;

        public GetPriceListItemByIdHandler(
            IGeneralRepository<PriceListItem> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<PriceListItemDto>> Handle(
            GetPriceListItemByIdQuery request,
            CancellationToken cancellationToken)
        {
            var item = await _repository
                .Get(x => x.Id == request.ItemId && x.PriceListId == request.PriceListId)
                .ProjectTo<PriceListItemDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (item == null)
            {
                throw new NotFoundException(
                    $"Price list item with ID {request.ItemId} not found in price list {request.PriceListId}",
                    FinanceErrorCode.NotFound);
            }

            return ResponseViewModel<PriceListItemDto>.Success(item, "Price list item retrieved successfully");
        }
    }

}
