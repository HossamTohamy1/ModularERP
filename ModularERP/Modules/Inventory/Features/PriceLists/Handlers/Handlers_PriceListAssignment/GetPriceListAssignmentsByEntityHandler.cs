using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListAssignment;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceListAssignment;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_PriceListAssignment
{
    public class GetPriceListAssignmentsByEntityHandler : IRequestHandler<GetPriceListAssignmentsByEntityQuery, ResponseViewModel<List<PriceListAssignmentDto>>>
    {
        private readonly IGeneralRepository<PriceListAssignment> _repository;
        private readonly IMapper _mapper;

        public GetPriceListAssignmentsByEntityHandler(
            IGeneralRepository<PriceListAssignment> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<List<PriceListAssignmentDto>>> Handle(
            GetPriceListAssignmentsByEntityQuery request,
            CancellationToken ct)
        {
            var result = await _repository
                .GetAll()
                .Where(x => x.EntityType == request.EntityType && x.EntityId == request.EntityId)
                .OrderByDescending(x => x.CreatedAt)
                .ProjectTo<PriceListAssignmentDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return ResponseViewModel<List<PriceListAssignmentDto>>.Success(
                result,
                "Price list assignments retrieved successfully");
        }
    }
}