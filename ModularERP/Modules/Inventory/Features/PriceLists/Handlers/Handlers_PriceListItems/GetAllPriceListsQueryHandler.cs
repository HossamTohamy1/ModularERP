using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceListItems;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_PriceListItems
{
    public class GetAllPriceListsQueryHandler : IRequestHandler<GetAllPriceListsQuery, ResponseViewModel<IEnumerable<PriceListListDto>>>
    {
        private readonly IGeneralRepository<PriceList> _repository;
        private readonly IMapper _mapper;

        public GetAllPriceListsQueryHandler(IGeneralRepository<PriceList> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<IEnumerable<PriceListListDto>>> Handle(GetAllPriceListsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _repository.GetAll();

                // Apply filters
                if (request.Filter.CompanyId.HasValue)
                {
                    query = query.Where(x => x.CompanyId == request.Filter.CompanyId.Value);
                }

                if (request.Filter.Type.HasValue)
                {
                    query = query.Where(x => x.Type == request.Filter.Type.Value);
                }

                if (request.Filter.Status.HasValue)
                {
                    query = query.Where(x => x.Status == request.Filter.Status.Value);
                }

                if (!string.IsNullOrEmpty(request.Filter.CurrencyCode))
                {
                    query = query.Where(x => x.CurrencyCode == request.Filter.CurrencyCode);
                }

                if (request.Filter.IsDefault.HasValue)
                {
                    query = query.Where(x => x.IsDefault == request.Filter.IsDefault.Value);
                }

                if (!string.IsNullOrEmpty(request.Filter.SearchTerm))
                {
                    query = query.Where(x => x.Name.Contains(request.Filter.SearchTerm));
                }

                // Pagination
                var totalCount = await query.CountAsync(cancellationToken);
                var priceLists = await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((request.Filter.PageNumber - 1) * request.Filter.PageSize)
                    .Take(request.Filter.PageSize)
                    .ProjectTo<PriceListListDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                return ResponseViewModel<IEnumerable<PriceListListDto>>.Success(
                    priceLists,
                    $"Retrieved {priceLists.Count} of {totalCount} price lists successfully"
                );
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(
                    $"Error retrieving price lists: {ex.Message}",
                    "Inventory"
                );
            }
        }
    }
}
