using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceRule;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handler_PriceRule
{
    public class ReorderPriceListRulesHandler : IRequestHandler<ReorderPriceListRulesCommand, Unit>
    {
        private readonly IGeneralRepository<PriceListRule> _ruleRepository;
        private readonly IGeneralRepository<PriceList> _priceListRepository;

        public ReorderPriceListRulesHandler(
            IGeneralRepository<PriceListRule> ruleRepository,
            IGeneralRepository<PriceList> priceListRepository)
        {
            _ruleRepository = ruleRepository;
            _priceListRepository = priceListRepository;
        }

        public async Task<Unit> Handle(ReorderPriceListRulesCommand request, CancellationToken cancellationToken)
        {
            // Validate Price List exists
            var priceListExists = await _priceListRepository.AnyAsync(pl => pl.Id == request.PriceListId);
            if (!priceListExists)
            {
                throw new NotFoundException(
                    $"Price list with ID {request.PriceListId} not found",
                    FinanceErrorCode.NotFound);
            }

            // Validate all rules exist and belong to the price list
            var ruleIds = request.Data.Rules.Select(r => r.RuleId).ToList();
            var existingRules = await _ruleRepository
                .Get(r => ruleIds.Contains(r.Id) && r.PriceListId == request.PriceListId)
                .ToListAsync(cancellationToken);

            if (existingRules.Count != ruleIds.Count)
            {
                throw new BusinessLogicException(
                    "One or more rules not found or do not belong to this price list",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            // Update priorities
            foreach (var ruleDto in request.Data.Rules)
            {
                var rule = existingRules.First(r => r.Id == ruleDto.RuleId);
                rule.Priority = ruleDto.Priority;
                rule.UpdatedAt = DateTime.UtcNow;
            }

            await _ruleRepository.SaveChanges();

            return Unit.Value;
        }
    }
}
