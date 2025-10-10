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
            var product = await _context.Products
                .Where(p => p.Id == request.ProductId && !p.IsDeleted)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.PurchasePrice,
                    p.SellingPrice
                })
                .FirstOrDefaultAsync(ct);

            if (product == null)
                throw new NotFoundException(
                    $"Product with ID {request.ProductId} not found",
                    FinanceErrorCode.NotFound);

            var transactionId = Guid.NewGuid();
            var steps = new List<PriceStepDTO>();
            decimal currentPrice = product.PurchasePrice ?? 0;
            int stepOrder = 0;

            // Base price step
            steps.Add(new PriceStepDTO
            {
                StepOrder = stepOrder++,
                RuleName = "Base Price",
                RuleType = "Base",
                ValueBefore = 0,
                ValueAfter = currentPrice,
                AdjustmentAmount = currentPrice
            });

            Guid? priceListId = request.PriceListId;

            // Try to get customer-specific price list
            if (priceListId == null && request.CustomerId.HasValue)
            {
                priceListId = await _context.Set<PriceListAssignment>()
                    .Where(a => a.EntityId == request.CustomerId.Value &&
                               a.EntityType == PriceListEntityType.Customer &&
                               !a.IsDeleted)
                    .Select(a => a.PriceListId)
                    .FirstOrDefaultAsync(ct);
            }

            // If still null, get default price list
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

            // 🔥 Validation: تحقق من وجود Price List
            if (!priceListId.HasValue)
            {
                throw new BusinessLogicException(
                    "No valid Price List found. Please provide a valid PriceListId or ensure a default price list exists.",
                    "Inventory",
                    FinanceErrorCode.ValidationError);
            }

            // 🔥 Validation: تحقق من وجود Rules للـ Price List
            var hasRules = await _context.Set<PriceListRule>()
                .AnyAsync(r => r.PriceListId == priceListId.Value && !r.IsDeleted, ct);

            if (!hasRules)
            {
                throw new BusinessLogicException(
                    $"The Price List (ID: {priceListId.Value}) must have at least one active Price Rule before calculating prices.",
                    "Inventory",
                    FinanceErrorCode.ValidationError);
            }

            // Apply price rules
            var rules = await _context.Set<PriceListRule>()
                .Where(r => r.PriceListId == priceListId.Value && !r.IsDeleted)
                .OrderBy(r => r.Priority)
                .Select(r => new
                {
                    r.Id,
                    r.RuleType,
                    r.Value
                })
                .ToListAsync(ct);

            foreach (var rule in rules)
            {
                decimal priceBefore = currentPrice;

                currentPrice = rule.RuleType switch
                {
                    PriceRuleType.Markup => currentPrice * (1 + (rule.Value ?? 0) / 100),
                    PriceRuleType.Margin => currentPrice / (1 - ((rule.Value ?? 0) / 100)),
                    PriceRuleType.FixedAdjustment => currentPrice + (rule.Value ?? 0),
                    _ => currentPrice
                };

                steps.Add(new PriceStepDTO
                {
                    StepOrder = stepOrder++,
                    RuleName = rule.RuleType.ToString(),
                    RuleType = rule.RuleType.ToString(),
                    ValueBefore = priceBefore,
                    ValueAfter = currentPrice,
                    AdjustmentAmount = currentPrice - priceBefore
                });

                if (request.CreateLog)
                {
                    await _logRepository.AddAsync(new PriceCalculationLog
                    {
                        TransactionId = transactionId,
                        TransactionType = request.TransactionType,
                        ProductId = request.ProductId,
                        AppliedRule = $"{rule.RuleType}",
                        ValueBefore = priceBefore,
                        ValueAfter = currentPrice,
                        UserId = request.UserId,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }

            // Apply bulk discounts
            var discounts = await _context.Set<BulkDiscount>()
                .Where(d => d.PriceListId == priceListId.Value &&
                           d.ProductId == request.ProductId &&
                           d.MinQty <= request.Quantity &&
                           (d.MaxQty == null || d.MaxQty >= request.Quantity) &&
                           !d.IsDeleted)
                .OrderByDescending(d => d.DiscountValue)
                .Select(d => new
                {
                    d.DiscountType,
                    d.DiscountValue
                })
                .FirstOrDefaultAsync(ct);

            if (discounts != null)
            {
                decimal priceBefore = currentPrice;
                decimal discountAmount = discounts.DiscountType == DiscountType.Percentage
                    ? currentPrice * (discounts.DiscountValue / 100)
                    : discounts.DiscountValue;

                currentPrice -= discountAmount;

                steps.Add(new PriceStepDTO
                {
                    StepOrder = stepOrder++,
                    RuleName = $"Bulk Discount (Qty: {request.Quantity})",
                    RuleType = "BulkDiscount",
                    ValueBefore = priceBefore,
                    ValueAfter = currentPrice,
                    AdjustmentAmount = -discountAmount
                });

                if (request.CreateLog)
                {
                    await _logRepository.AddAsync(new PriceCalculationLog
                    {
                        TransactionId = transactionId,
                        TransactionType = request.TransactionType,
                        ProductId = request.ProductId,
                        AppliedRule = $"Bulk Discount - {discounts.DiscountType}",
                        ValueBefore = priceBefore,
                        ValueAfter = currentPrice,
                        UserId = request.UserId,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }

            if (request.CreateLog)
            {
                await _logRepository.SaveChanges();
            }

            var totalDiscount = steps
                .Where(s => s.AdjustmentAmount < 0)
                .Sum(s => Math.Abs(s.AdjustmentAmount));

            return new PriceCalculationResultDTO
            {
                TransactionId = request.CreateLog ? transactionId : null,
                ProductId = product.Id,
                ProductName = product.Name,
                BasePrice = product.PurchasePrice ?? 0,
                FinalPrice = currentPrice,
                TotalDiscount = totalDiscount,
                TotalTax = 0,
                CalculationSteps = steps,
                CalculatedAt = DateTime.UtcNow
            };
        }
    }
}