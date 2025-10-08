using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceRule;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handler_PriceRule
{
    public class DeletePriceListRuleHandler : IRequestHandler<DeletePriceListRuleCommand, Unit>
    {
        private readonly IGeneralRepository<PriceListRule> _ruleRepository;

        public DeletePriceListRuleHandler(IGeneralRepository<PriceListRule> ruleRepository)
        {
            _ruleRepository = ruleRepository;
        }

        public async Task<Unit> Handle(DeletePriceListRuleCommand request, CancellationToken cancellationToken)
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

            await _ruleRepository.Delete(request.RuleId);

            return Unit.Value;
        }
    }
}
