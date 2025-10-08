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
    public class GetPriceListRuleByIdHandler : IRequestHandler<GetPriceListRuleByIdQuery, PriceListRuleResponseDTO>
    {
        private readonly IGeneralRepository<PriceListRule> _ruleRepository;
        private readonly IMapper _mapper;

        public GetPriceListRuleByIdHandler(
            IGeneralRepository<PriceListRule> ruleRepository,
            IMapper mapper)
        {
            _ruleRepository = ruleRepository;
            _mapper = mapper;
        }

        public async Task<PriceListRuleResponseDTO> Handle(GetPriceListRuleByIdQuery request, CancellationToken cancellationToken)
        {
            var rule = await _ruleRepository
                .Get(r => r.Id == request.RuleId && r.PriceListId == request.PriceListId)
                .ProjectTo<PriceListRuleResponseDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (rule == null)
            {
                throw new NotFoundException(
                    $"Rule with ID {request.RuleId} not found in price list {request.PriceListId}",
                    FinanceErrorCode.NotFound);
            }

            return rule;
        }
    }
}
