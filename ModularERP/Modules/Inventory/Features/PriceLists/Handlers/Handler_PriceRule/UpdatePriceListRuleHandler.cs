using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceRule;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceRule;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handler_PriceRule
{
    public class UpdatePriceListRuleHandler : IRequestHandler<UpdatePriceListRuleCommand, PriceListRuleResponseDTO>
    {
        private readonly IGeneralRepository<PriceListRule> _ruleRepository;
        private readonly IMapper _mapper;

        public UpdatePriceListRuleHandler(
            IGeneralRepository<PriceListRule> ruleRepository,
            IMapper mapper)
        {
            _ruleRepository = ruleRepository;
            _mapper = mapper;
        }

        public async Task<PriceListRuleResponseDTO> Handle(UpdatePriceListRuleCommand request, CancellationToken cancellationToken)
        {
            var rule = await _ruleRepository
                .Get(r => r.Id == request.RuleId && r.PriceListId == request.PriceListId)
                .FirstOrDefaultAsync(cancellationToken);

            if (rule == null)
            {
                throw new NotFoundException(
                    $"Rule with ID {request.RuleId} not found in price list {request.PriceListId}",
                    FinanceErrorCode.NotFound);
            }

            // Map updates
            _mapper.Map(request.Data, rule);
            rule.UpdatedAt = DateTime.UtcNow;

            await _ruleRepository.Update(rule);

            return _mapper.Map<PriceListRuleResponseDTO>(rule);
        }
    }

}
