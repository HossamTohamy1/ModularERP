using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceListItems;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_PriceListItems
{
    public class GetPriceListsByCustomerQueryHandler : IRequestHandler<GetPriceListsByCustomerQuery, ResponseViewModel<IEnumerable<PriceListListDto>>>
    {
        private readonly IGeneralRepository<PriceList> _repository;
        private readonly IGeneralRepository<PriceListAssignment> _assignmentRepository;
        private readonly IMapper _mapper;

        public GetPriceListsByCustomerQueryHandler(
            IGeneralRepository<PriceList> repository,
            IGeneralRepository<PriceListAssignment> assignmentRepository,
            IMapper mapper)
        {
            _repository = repository;
            _assignmentRepository = assignmentRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<IEnumerable<PriceListListDto>>> Handle(GetPriceListsByCustomerQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var assignedPriceListIds = await _assignmentRepository
                    .GetAll()
                    .Where(x => x.EntityId == request.CustomerId &&
                               (x.EntityType == PriceListEntityType.Customer ||
                                x.EntityType == PriceListEntityType.CustomerGroup))
                    .Select(x => x.PriceListId)
                    .ToListAsync(cancellationToken);

                var priceLists = await _repository
                    .GetAll()
                    .Where(x => assignedPriceListIds.Contains(x.Id) && x.Status == PriceListStatus.Active)
                    .ProjectTo<PriceListListDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                return ResponseViewModel<IEnumerable<PriceListListDto>>.Success(
                    priceLists,
                    $"Retrieved {priceLists.Count} price lists for customer successfully"
                );
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(
                    $"Error retrieving customer price lists: {ex.Message}",
                    "Inventory"
                );
            }
        }
    }
}
