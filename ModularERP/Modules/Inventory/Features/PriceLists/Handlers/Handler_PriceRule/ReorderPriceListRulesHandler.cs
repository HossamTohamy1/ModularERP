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
            // 1. Validate Price List exists
            var priceListExists = await _priceListRepository.AnyAsync(pl => pl.Id == request.PriceListId);
            if (!priceListExists)
            {
                throw new NotFoundException(
                    $"Price list with ID {request.PriceListId} not found",
                    FinanceErrorCode.NotFound);
            }

            // 2. Validate input data
            if (request.Data?.Rules == null || !request.Data.Rules.Any())
            {
                throw new BusinessLogicException(
                    "Rules list cannot be empty",
                    "Inventory",
                    FinanceErrorCode.ValidationError);
            }

            // 3. Check for duplicate RuleIds in request
            var duplicateRuleIds = request.Data.Rules
                .GroupBy(r => r.RuleId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateRuleIds.Any())
            {
                throw new BusinessLogicException(
                    $"Duplicate rule IDs found: {string.Join(", ", duplicateRuleIds)}",
                    "Inventory",
                    FinanceErrorCode.ValidationError);
            }

            // 4. Check for duplicate Priorities in request
            var duplicatePriorities = request.Data.Rules
                .GroupBy(r => r.Priority)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatePriorities.Any())
            {
                throw new BusinessLogicException(
                    $"Duplicate priorities found: {string.Join(", ", duplicatePriorities)}",
                    "Inventory",
                    FinanceErrorCode.ValidationError);
            }

            // 5. Validate Priority values
            foreach (var ruleDto in request.Data.Rules)
            {
                if (ruleDto.Priority < 1)
                {
                    throw new BusinessLogicException(
                        $"Priority must be greater than 0 for rule {ruleDto.RuleId}",
                        "Inventory",
                        FinanceErrorCode.ValidationError);
                }
            }

            // 6. Get all existing rules for this price list
            var ruleIds = request.Data.Rules.Select(r => r.RuleId).ToList();
            var existingRules = await _ruleRepository
                .Get(r => r.PriceListId == request.PriceListId && !r.IsDeleted)
                .ToListAsync(cancellationToken);

            // 7. Validate all requested rules exist and belong to price list
            var requestedRules = existingRules
                .Where(r => ruleIds.Contains(r.Id))
                .ToList();

            if (requestedRules.Count != ruleIds.Count)
            {
                var missingIds = ruleIds.Except(requestedRules.Select(r => r.Id)).ToList();
                throw new BusinessLogicException(
                    $"Rules not found or do not belong to this price list: {string.Join(", ", missingIds)}",
                    "Inventory",
                    FinanceErrorCode.NotFound);
            }

            // 8. Get rules NOT in the reorder request
            var otherRules = existingRules
                .Where(r => !ruleIds.Contains(r.Id))
                .ToList();

            // 9. Check for priority conflicts with other rules
            var newPriorities = request.Data.Rules.Select(r => r.Priority).ToHashSet();
            var conflictingRules = otherRules
                .Where(r => newPriorities.Contains(r.Priority))
                .ToList();

            if (conflictingRules.Any())
            {
                var conflicts = conflictingRules
                    .Select(r => $"Rule {r.Id} (Priority {r.Priority})")
                    .ToList();

                throw new BusinessLogicException(
                    $"Priority conflicts detected with existing rules: {string.Join(", ", conflicts)}",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            // 10. Update priorities for requested rules
            foreach (var ruleDto in request.Data.Rules)
            {
                var rule = requestedRules.First(r => r.Id == ruleDto.RuleId);
                rule.Priority = ruleDto.Priority;
                rule.UpdatedAt = DateTime.UtcNow;
            }

            await _ruleRepository.SaveChanges();

            return Unit.Value;
        }
    }
}