using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceListItems;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_PriceListItems
{
    public class GetPriceListByIdQueryHandler : IRequestHandler<GetPriceListByIdQuery, ResponseViewModel<PriceListDto>>
    {
        private readonly IGeneralRepository<PriceList> _repository;
        private readonly IMapper _mapper;

        public GetPriceListByIdQueryHandler(IGeneralRepository<PriceList> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<PriceListDto>> Handle(GetPriceListByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var priceList = await _repository
                    .GetAll()
                    .Where(x => x.Id == request.Id)
                    .ProjectTo<PriceListDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                if (priceList == null)
                {
                    throw new NotFoundException(
                        $"Price list with ID {request.Id} not found",
                        FinanceErrorCode.NotFound
                    );
                }

                return ResponseViewModel<PriceListDto>.Success(priceList, "Price list retrieved successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(
                    $"Error retrieving price list: {ex.Message}",
                    "Inventory"
                );
            }
        }
    }

}
