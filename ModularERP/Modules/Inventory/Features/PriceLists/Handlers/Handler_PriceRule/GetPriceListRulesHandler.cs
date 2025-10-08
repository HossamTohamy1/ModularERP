using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceRule;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceRule;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handler_PriceRule
{
    public class GetPriceListRulesHandler : IRequestHandler<GetPriceListRulesQuery, List<PriceListRuleResponseDTO>>
    {
        private readonly IGeneralRepository<PriceListRule> _ruleRepository;
        private readonly IGeneralRepository<PriceList> _priceListRepository;
        private readonly IMapper _mapper;

        public GetPriceListRulesHandler(
            IGeneralRepository<PriceListRule> ruleRepository,
            IGeneralRepository<PriceList> priceListRepository,
            IMapper mapper)
        {
            _ruleRepository = ruleRepository;
            _priceListRepository = priceListRepository;
            _mapper = mapper;
        }

        public async Task<List<PriceListRuleResponseDTO>> Handle(GetPriceListRulesQuery request, CancellationToken cancellationToken)
        {
            // Validate Price List exists
            var priceListExists = await _priceListRepository.AnyAsync(pl => pl.Id == request.PriceListId);
            if (!priceListExists)
            {
                throw new NotFoundException(
                    $"Price list with ID {request.PriceListId} not found",
                    FinanceErrorCode.NotFound);
            }

            // Get rules with projection
            return await _ruleRepository
                .Get(r => r.PriceListId == request.PriceListId)
                .OrderBy(r => r.Priority)
                .ProjectTo<PriceListRuleResponseDTO>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
