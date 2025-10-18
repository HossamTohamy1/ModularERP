using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceRule;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceRule;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handler_PriceRule
{
    public class CreatePriceListRuleHandler : IRequestHandler<CreatePriceListRuleCommand, PriceListRuleResponseDTO>
    {
        private readonly IGeneralRepository<PriceListRule> _ruleRepository;
        private readonly IGeneralRepository<PriceList> _priceListRepository;
        private readonly IMapper _mapper;

        public CreatePriceListRuleHandler(
            IGeneralRepository<PriceListRule> ruleRepository,
            IGeneralRepository<PriceList> priceListRepository,
            IMapper mapper)
        {
            _ruleRepository = ruleRepository;
            _priceListRepository = priceListRepository;
            _mapper = mapper;
        }

        public async Task<PriceListRuleResponseDTO> Handle(CreatePriceListRuleCommand request, CancellationToken cancellationToken)
        {
            // 1. Validate Price List exists
            var priceListExists = await _priceListRepository.AnyAsync(pl => pl.Id == request.PriceListId);
            if (!priceListExists)
            {
                throw new NotFoundException(
                    $"Price list with ID {request.PriceListId} not found",
                    FinanceErrorCode.NotFound);
            }

            // 2. Validate Priority uniqueness
            var priorityExists = await _ruleRepository.AnyAsync(r =>
                r.PriceListId == request.PriceListId &&
                r.Priority == request.Data.Priority &&
                !r.IsDeleted);

            if (priorityExists)
            {
                throw new BusinessLogicException(
                    $"A rule with Priority {request.Data.Priority} already exists for this price list",
                    "Inventory",
                    FinanceErrorCode.DuplicateEntity);
            }

            // 3. Validate Value based on RuleType
            ValidateRuleValue(request.Data.RuleType, request.Data.Value);

            // 4. Validate Date Range
            if (request.Data.StartDate.HasValue && request.Data.EndDate.HasValue)
            {
                if (request.Data.EndDate.Value <= request.Data.StartDate.Value)
                {
                    throw new BusinessLogicException(
                        "End date must be after start date",
                        "Inventory",
                        FinanceErrorCode.ValidationError);
                }
            }

            // 5. Check for overlapping date ranges with same priority
            if (request.Data.StartDate.HasValue || request.Data.EndDate.HasValue)
            {
                var hasOverlap = await _ruleRepository
                    .Get(r => r.PriceListId == request.PriceListId &&
                             r.Priority == request.Data.Priority &&
                             !r.IsDeleted)
                    .AnyAsync(r =>
                        (request.Data.StartDate == null || r.EndDate == null || request.Data.StartDate <= r.EndDate) &&
                        (request.Data.EndDate == null || r.StartDate == null || request.Data.EndDate >= r.StartDate),
                        cancellationToken);

                if (hasOverlap)
                {
                    throw new BusinessLogicException(
                        "Date range overlaps with existing rule at the same priority",
                        "Inventory",
                        FinanceErrorCode.BusinessLogicError);
                }
            }

            // 6. Map and create entity
            var rule = _mapper.Map<PriceListRule>(request.Data);
            rule.PriceListId = request.PriceListId;
            rule.CreatedAt = DateTime.UtcNow;

            await _ruleRepository.AddAsync(rule);
            await _ruleRepository.SaveChanges();

            return _mapper.Map<PriceListRuleResponseDTO>(rule);
        }

        private void ValidateRuleValue(PriceRuleType ruleType, decimal? value)
        {
            if (!value.HasValue)
            {
                throw new BusinessLogicException(
                    "Rule value is required",
                    "Inventory",
                    FinanceErrorCode.ValidationError);
            }

            switch (ruleType)
            {
                case PriceRuleType.Markup:
                    if (value.Value < 0)
                    {
                        throw new BusinessLogicException(
                            "Markup percentage cannot be negative",
                            "Inventory",
                            FinanceErrorCode.ValidationError);
                    }
                    break;

                case PriceRuleType.Margin:
                    if (value.Value <= 0 || value.Value >= 100)
                    {
                        throw new BusinessLogicException(
                            "Margin percentage must be between 0 and 100",
                            "Inventory",
                            FinanceErrorCode.ValidationError);
                    }
                    break;

                case PriceRuleType.FixedAdjustment:
                    // Can be positive or negative
                    break;

                case PriceRuleType.CurrencyConversion:
                    if (value.Value <= 0)
                    {
                        throw new BusinessLogicException(
                            "Currency conversion rate must be positive",
                            "Inventory",
                            FinanceErrorCode.ValidationError);
                    }
                    break;

                case PriceRuleType.Promotion:
                    if (value.Value < 0 || value.Value > 100)
                    {
                        throw new BusinessLogicException(
                            "Promotion discount must be between 0 and 100",
                            "Inventory",
                            FinanceErrorCode.ValidationError);
                    }
                    break;

                default:
                    throw new BusinessLogicException(
                        $"Unknown rule type: {ruleType}",
                        "Inventory",
                        FinanceErrorCode.ValidationError);
            }
        }
    }
}