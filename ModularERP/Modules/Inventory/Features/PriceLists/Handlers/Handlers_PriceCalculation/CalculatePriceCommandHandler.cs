using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commands_PriceCalculation;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceCalculation;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_PriceCalculation
{
    public class CalculatePriceCommandHandler : IRequestHandler<CalculatePriceCommand, PriceCalculationResultDTO>
    {
        private readonly FinanceDbContext _context;
        private readonly IMapper _mapper;
        private readonly IGeneralRepository<PriceCalculationLog> _logRepository;

        public CalculatePriceCommandHandler(
            FinanceDbContext context,
            IMapper mapper,
            IGeneralRepository<PriceCalculationLog> logRepository)
        {
            _context = context;
            _mapper = mapper;
            _logRepository = logRepository;
        }

        public async Task<PriceCalculationResultDTO> Handle(CalculatePriceCommand request, CancellationToken ct)
        {
            if (request.Quantity <= 0)
                throw new BusinessLogicException(
                    "Quantity must be greater than zero",
                    "Inventory",
                    FinanceErrorCode.ValidationError);

            // Get product with TaxProfiles
            var product = await _context.Products
                .Where(p => p.Id == request.ProductId && !p.IsDeleted)
                .Include(p => p.ProductTaxProfiles.Where(ptp => !ptp.IsDeleted))
                    .ThenInclude(ptp => ptp.TaxProfile)
                        .ThenInclude(tp => tp.TaxProfileComponents.Where(tpc => !tpc.IsDeleted))
                            .ThenInclude(tpc => tpc.TaxComponent)
                .FirstOrDefaultAsync(ct);

            if (product == null)
                throw new NotFoundException(
                    $"Product with ID {request.ProductId} not found",
                    FinanceErrorCode.NotFound);

            var transactionId = Guid.NewGuid();
            var steps = new List<PriceStepDTO>();
            decimal currentPrice = product.PurchasePrice ?? product.SellingPrice ?? 0;
            int stepOrder = 0;

            // Base Price Step
            steps.Add(new PriceStepDTO
            {
                StepOrder = stepOrder++,
                RuleName = "Base Price",
                RuleType = "Base",
                ValueBefore = 0,
                ValueAfter = currentPrice,
                AdjustmentAmount = currentPrice
            });

            // Get PriceList
            Guid? priceListId = await GetPriceListId(request, ct);
            await ValidatePriceList(priceListId, ct);

            // Check PriceListItem override
            var priceListItem = await _context.Set<PriceListItem>()
                .Where(i => i.PriceListId == priceListId.Value &&
                            i.ProductId == request.ProductId &&
                            !i.IsDeleted &&
                            (i.ValidFrom == null || i.ValidFrom <= DateTime.UtcNow) &&
                            (i.ValidTo == null || i.ValidTo >= DateTime.UtcNow))
                .FirstOrDefaultAsync(ct);

            if (priceListItem != null && priceListItem.BasePrice.HasValue)
            {
                decimal priceBefore = currentPrice;
                currentPrice = priceListItem.BasePrice.Value;

                steps.Add(new PriceStepDTO
                {
                    StepOrder = stepOrder++,
                    RuleName = "Price List Item Override",
                    RuleType = "Override",
                    ValueBefore = priceBefore,
                    ValueAfter = currentPrice,
                    AdjustmentAmount = currentPrice - priceBefore
                });
            }

            // Apply Price Rules
            var rules = await _context.Set<PriceListRule>()
                .Where(r => r.PriceListId == priceListId.Value &&
                           !r.IsDeleted &&
                           (r.StartDate == null || r.StartDate <= DateTime.UtcNow) &&
                           (r.EndDate == null || r.EndDate >= DateTime.UtcNow))
                .OrderBy(r => r.Priority)
                .ToListAsync(ct);

            foreach (var rule in rules)
            {
                decimal priceBefore = currentPrice;
                currentPrice = ApplyRule(currentPrice, rule);

                steps.Add(new PriceStepDTO
                {
                    StepOrder = stepOrder++,
                    RuleName = $"{rule.RuleType} ({rule.Value}%)",
                    RuleType = rule.RuleType.ToString(),
                    ValueBefore = priceBefore,
                    ValueAfter = currentPrice,
                    AdjustmentAmount = currentPrice - priceBefore
                });

                if (request.CreateLog)
                {
                    await LogStep(transactionId, request, rule.RuleType.ToString(), priceBefore, currentPrice);
                }
            }

            // Apply Bulk Discounts
            (currentPrice, stepOrder) = await ApplyBulkDiscounts(
                priceListId.Value,
                request.ProductId,
                request.Quantity,
                currentPrice,
                steps,
                stepOrder,
                transactionId,
                request,
                ct);

            // Apply Tax using ProductTaxProfiles
            decimal totalTax = 0;
            if (product.ProductTaxProfiles.Any() || priceListItem?.TaxProfileId != null)
            {
                var taxProfiles = product.ProductTaxProfiles.Select(ptp => ptp.TaxProfile).ToList();

                foreach (var tp in taxProfiles)
                {
                    (decimal taxAmount, int newStepOrder) = await ApplyTax(tp, currentPrice, steps, stepOrder, ct);
                    totalTax += taxAmount;
                    stepOrder = newStepOrder;
                    currentPrice += taxAmount;
                }

                // If PriceListItem has TaxProfile override
                if (priceListItem?.TaxProfileId != null)
                {
                    var taxProfile = await _context.Set<TaxProfile>()
                        .Include(tp => tp.TaxProfileComponents.Where(c => !c.IsDeleted))
                            .ThenInclude(tpc => tpc.TaxComponent)
                        .FirstOrDefaultAsync(tp => tp.Id == priceListItem.TaxProfileId, ct);

                    if (taxProfile != null)
                    {
                        (decimal taxAmount, int newStepOrder) = await ApplyTax(taxProfile, currentPrice, steps, stepOrder, ct);
                        totalTax += taxAmount;
                        stepOrder = newStepOrder;
                        currentPrice += taxAmount;
                    }
                }
            }

            if (request.CreateLog)
                await _logRepository.SaveChanges();

            var totalDiscount = steps
                .Where(s => s.RuleType == "BulkDiscount" || s.RuleType == "Promotion")
                .Sum(s => Math.Abs(s.AdjustmentAmount));

            return new PriceCalculationResultDTO
            {
                TransactionId = request.CreateLog ? transactionId : null,
                ProductId = product.Id,
                ProductName = product.Name,
                BasePrice = product.PurchasePrice ?? product.SellingPrice ?? 0,
                FinalPrice = currentPrice,
                TotalDiscount = totalDiscount,
                TotalTax = totalTax,
                CalculationSteps = steps,
                CalculatedAt = DateTime.UtcNow
            };
        }

        private async Task<Guid?> GetPriceListId(CalculatePriceCommand request, CancellationToken ct)
        {
            Guid? priceListId = request.PriceListId;

            if (priceListId == null && request.CustomerId.HasValue)
            {
                priceListId = await _context.Set<PriceListAssignment>()
                    .Where(a => a.EntityId == request.CustomerId.Value &&
                               a.EntityType == PriceListEntityType.Customer &&
                               !a.IsDeleted)
                    .Select(a => a.PriceListId)
                    .FirstOrDefaultAsync(ct);
            }

            if (priceListId == null)
            {
                priceListId = await _context.PriceLists
                    .Where(pl => pl.IsDefault &&
                                pl.Type == PriceListType.Sales &&
                                pl.Status == PriceListStatus.Active &&
                                !pl.IsDeleted)
                    .Select(pl => pl.Id)
                    .FirstOrDefaultAsync(ct);
            }

            return priceListId;
        }

        private async Task ValidatePriceList(Guid? priceListId, CancellationToken ct)
        {
            if (!priceListId.HasValue)
                throw new BusinessLogicException(
                    "No valid Price List found.",
                    "Inventory",
                    FinanceErrorCode.ValidationError);

            var hasRules = await _context.Set<PriceListRule>()
                .AnyAsync(r => r.PriceListId == priceListId.Value && !r.IsDeleted, ct);

            if (!hasRules)
                throw new BusinessLogicException(
                    $"The Price List (ID: {priceListId.Value}) must have at least one active Price Rule.",
                    "Inventory",
                    FinanceErrorCode.ValidationError);
        }

        private decimal ApplyRule(decimal currentPrice, PriceListRule rule)
        {
            return rule.RuleType switch
            {
                PriceRuleType.Markup => currentPrice * (1 + (rule.Value ?? 0) / 100),
                PriceRuleType.Margin => currentPrice / (1 - ((rule.Value ?? 0) / 100)),
                PriceRuleType.FixedAdjustment => currentPrice + (rule.Value ?? 0),
                PriceRuleType.CurrencyConversion => currentPrice * (rule.Value ?? 1),
                PriceRuleType.Promotion => currentPrice * (1 - (rule.Value ?? 0) / 100),
                _ => currentPrice
            };
        }

        private async Task<(decimal newPrice, int newStepOrder)> ApplyBulkDiscounts(
            Guid priceListId,
            Guid productId,
            decimal quantity,
            decimal currentPrice,
            List<PriceStepDTO> steps,
            int stepOrder,
            Guid transactionId,
            CalculatePriceCommand request,
            CancellationToken ct)
        {
            var discount = await _context.Set<BulkDiscount>()
                .Where(d => d.PriceListId == priceListId &&
                            d.ProductId == productId &&
                            d.MinQty <= quantity &&
                            (d.MaxQty == null || d.MaxQty >= quantity) &&
                            !d.IsDeleted)
                .OrderByDescending(d => d.DiscountValue)
                .FirstOrDefaultAsync(ct);

            if (discount != null)
            {
                decimal priceBefore = currentPrice;
                decimal discountAmount = discount.DiscountType == DiscountType.Percentage
                    ? currentPrice * (discount.DiscountValue / 100)
                    : discount.DiscountValue;

                currentPrice -= discountAmount;

                steps.Add(new PriceStepDTO
                {
                    StepOrder = stepOrder,
                    RuleName = $"Bulk Discount (Qty: {quantity})",
                    RuleType = "BulkDiscount",
                    ValueBefore = priceBefore,
                    ValueAfter = currentPrice,
                    AdjustmentAmount = -discountAmount
                });

                if (request.CreateLog)
                    await LogStep(transactionId, request, $"Bulk Discount - {discount.DiscountType}", priceBefore, currentPrice);

                stepOrder++;
            }

            return (currentPrice, stepOrder);
        }

        private async Task<(decimal, int)> ApplyTax(
            TaxProfile taxProfile,
            decimal currentPrice,
            List<PriceStepDTO> steps,
            int stepOrder,
            CancellationToken ct)
        {
            decimal totalTax = 0;

            foreach (var component in taxProfile.TaxProfileComponents.OrderBy(c => c.Priority))
            {
                decimal taxAmount = component.TaxComponent.RateType == TaxRateType.Percentage
                    ? currentPrice * (component.TaxComponent.RateValue / 100)
                    : component.TaxComponent.RateValue;

                steps.Add(new PriceStepDTO
                {
                    StepOrder = stepOrder++,
                    RuleName = $"Tax - {component.TaxComponent.Name} ({component.TaxComponent.RateValue}{(component.TaxComponent.RateType == TaxRateType.Percentage ? "%" : "")})",
                    RuleType = "Tax",
                    ValueBefore = currentPrice,
                    ValueAfter = currentPrice + taxAmount,
                    AdjustmentAmount = taxAmount
                });

                currentPrice += taxAmount;
                totalTax += taxAmount;
            }

            return (totalTax, stepOrder);
        }

        private async Task LogStep(
            Guid transactionId,
            CalculatePriceCommand request,
            string ruleName,
            decimal valueBefore,
            decimal valueAfter)
        {
            await _logRepository.AddAsync(new PriceCalculationLog
            {
                TransactionId = transactionId,
                TransactionType = request.TransactionType,
                ProductId = request.ProductId,
                AppliedRule = ruleName,
                ValueBefore = valueBefore,
                ValueAfter = valueAfter,
                UserId = request.UserId,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
