using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceCalculation;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceCalculation;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_PriceCalculation
{
    public class GetPriceBreakdownQueryHandler : IRequestHandler<GetPriceBreakdownQuery, PriceBreakdownDTO>
    {
        private readonly IGeneralRepository<PriceCalculationLog> _logRepository;
        private readonly FinanceDbContext _context;
        private readonly IMapper _mapper;

        public GetPriceBreakdownQueryHandler(
            IGeneralRepository<PriceCalculationLog> logRepository,
            FinanceDbContext context,
            IMapper mapper)
        {
            _logRepository = logRepository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<PriceBreakdownDTO> Handle(GetPriceBreakdownQuery request, CancellationToken ct)
        {
            // Get logs using AutoMapper projection
            var logs = await _logRepository
                .Get(l => l.TransactionId == request.TransactionId)
                .OrderBy(l => l.Timestamp)
                .ProjectTo<PriceCalculationLogDTO>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            if (!logs.Any())
                throw new NotFoundException(
                    $"No price calculation found for transaction {request.TransactionId}",
                    FinanceErrorCode.NotFound);

            var firstLog = logs.First();
            var lastLog = logs.Last();

            // Get the original log to access UserId
            var originalFirstLog = await _logRepository
                .Get(l => l.TransactionId == request.TransactionId)
                .OrderBy(l => l.Timestamp)
                .Select(l => new
                {
                    l.ProductId,
                    l.UserId
                })
                .FirstOrDefaultAsync(ct);

            // Get product details using projection
            var product = await _context.Products
                .Where(p => p.Id == firstLog.ProductId && !p.IsDeleted)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.SKU
                })
                .FirstOrDefaultAsync(ct);

            // Get user details using projection
            var calculatedBy = originalFirstLog?.UserId != null
                ? await _context.Users
                    .Where(u => u.Id == originalFirstLog.UserId.Value)
                    .Select(u => u.UserName ?? u.Email ?? "Unknown")
                    .FirstOrDefaultAsync(ct) ?? "System"
                : "System";

            // Calculate price after markup (before discounts)
            var markupLogs = logs
                .Where(l => l.AppliedRule != null &&
                           (l.AppliedRule.Contains("Markup") ||
                            l.AppliedRule.Contains("Margin") ||
                            l.AppliedRule.Contains("Fixed")))
                .ToList();

            var priceAfterMarkup = markupLogs.Any()
                ? markupLogs.Last().ValueAfter ?? firstLog.ValueBefore ?? 0
                : firstLog.ValueBefore ?? 0;

            // Calculate price after discounts (before tax)
            var discountLogs = logs
                .Where(l => l.AppliedRule != null && l.AppliedRule.Contains("Discount"))
                .ToList();

            var priceAfterDiscount = discountLogs.Any()
                ? discountLogs.Last().ValueAfter ?? priceAfterMarkup
                : priceAfterMarkup;

            // Calculate price before tax
            var taxLogs = logs
                .Where(l => l.AppliedRule != null && l.AppliedRule.Contains("Tax"))
                .ToList();

            var priceBeforeTax = taxLogs.Any()
                ? taxLogs.First().ValueBefore ?? priceAfterDiscount
                : priceAfterDiscount;

            // Map applied rules
            var appliedRules = logs
                .Where(l => l.AppliedRule != null &&
                           !l.AppliedRule.Contains("Discount") &&
                           !l.AppliedRule.Contains("Tax"))
                .Select(l => new RuleApplicationDTO
                {
                    RuleName = l.AppliedRule ?? string.Empty,
                    RuleType = ExtractRuleType(l.AppliedRule),
                    RuleValue = CalculateRuleValue(l.ValueBefore ?? 0, l.ValueAfter ?? 0, l.AppliedRule),
                    PriceBefore = l.ValueBefore ?? 0,
                    PriceAfter = l.ValueAfter ?? 0,
                    Impact = (l.ValueAfter ?? 0) - (l.ValueBefore ?? 0)
                })
                .ToList();

            // Map applied discounts
            var appliedDiscounts = logs
                .Where(l => l.AppliedRule != null && l.AppliedRule.Contains("Discount"))
                .Select(l => new DiscountApplicationDTO
                {
                    DiscountType = ExtractDiscountType(l.AppliedRule),
                    DiscountAmount = Math.Abs((l.ValueAfter ?? 0) - (l.ValueBefore ?? 0)),
                    Reason = l.AppliedRule ?? string.Empty
                })
                .ToList();

            // Map applied taxes
            var appliedTaxes = logs
                .Where(l => l.AppliedRule != null && l.AppliedRule.Contains("Tax"))
                .Select(l => new TaxApplicationDTO
                {
                    TaxName = ExtractTaxName(l.AppliedRule),
                    TaxRate = CalculateTaxRate(l.ValueBefore ?? 0, l.ValueAfter ?? 0),
                    TaxAmount = (l.ValueAfter ?? 0) - (l.ValueBefore ?? 0),
                    IsInclusive = false
                })
                .ToList();

            var totalDiscount = appliedDiscounts.Sum(d => d.DiscountAmount);
            var totalTax = appliedTaxes.Sum(t => t.TaxAmount);

            // Extract quantity from logs
            var quantity = GetQuantityFromLogs(logs);

            return new PriceBreakdownDTO
            {
                TransactionId = request.TransactionId,
                TransactionType = firstLog.TransactionType ?? string.Empty,
                ProductId = firstLog.ProductId ?? Guid.Empty,
                ProductName = product?.Name ?? string.Empty,
                ProductSKU = product?.SKU ?? string.Empty,
                Quantity = quantity,
                BasePrice = firstLog.ValueBefore ?? 0,
                PriceAfterMarkup = priceAfterMarkup,
                PriceAfterDiscount = priceAfterDiscount,
                PriceBeforeTax = priceBeforeTax,
                FinalPrice = lastLog.ValueAfter ?? 0,
                AppliedRules = appliedRules,
                AppliedDiscounts = appliedDiscounts,
                AppliedTaxes = appliedTaxes,
                TotalDiscount = totalDiscount,
                TotalTax = totalTax,
                TotalAmount = lastLog.ValueAfter ?? 0,
                CalculatedAt = firstLog.Timestamp,
                CalculatedBy = calculatedBy
            };
        }

        private string ExtractRuleType(string? appliedRule)
        {
            if (string.IsNullOrEmpty(appliedRule)) return "Unknown";

            if (appliedRule.Contains("Markup")) return "Markup";
            if (appliedRule.Contains("Margin")) return "Margin";
            if (appliedRule.Contains("Fixed")) return "FixedAdjustment";

            return "Custom";
        }

        private string ExtractDiscountType(string? appliedRule)
        {
            if (string.IsNullOrEmpty(appliedRule)) return "Unknown";

            if (appliedRule.Contains("Bulk")) return "Bulk";
            if (appliedRule.Contains("Percentage")) return "Percentage";
            if (appliedRule.Contains("Fixed")) return "Fixed";

            return "General";
        }

        private string ExtractTaxName(string? appliedRule)
        {
            if (string.IsNullOrEmpty(appliedRule)) return "Tax";

            var parts = appliedRule.Split('-', '(');
            return parts[0].Trim();
        }

        private decimal CalculateRuleValue(decimal priceBefore, decimal priceAfter, string? appliedRule)
        {
            if (priceBefore == 0 || string.IsNullOrEmpty(appliedRule)) return 0;

            var difference = priceAfter - priceBefore;

            if (appliedRule.Contains("Markup"))
            {
                return (difference / priceBefore) * 100;
            }
            else if (appliedRule.Contains("Margin"))
            {
                return priceAfter != 0 ? (difference / priceAfter) * 100 : 0;
            }
            else if (appliedRule.Contains("Fixed"))
            {
                return difference;
            }

            return 0;
        }

        private decimal CalculateTaxRate(decimal taxableAmount, decimal amountAfterTax)
        {
            if (taxableAmount == 0) return 0;

            var taxAmount = amountAfterTax - taxableAmount;
            return (taxAmount / taxableAmount) * 100;
        }

        private int GetQuantityFromLogs(List<PriceCalculationLogDTO> logs)
        {
            var bulkDiscountLog = logs.FirstOrDefault(l =>
                l.AppliedRule != null && l.AppliedRule.Contains("Qty:"));

            if (bulkDiscountLog != null && bulkDiscountLog.AppliedRule != null)
            {
                var startIndex = bulkDiscountLog.AppliedRule.IndexOf("Qty:") + 4;
                var endIndex = bulkDiscountLog.AppliedRule.IndexOf(")", startIndex);

                if (startIndex > 3 && endIndex > startIndex)
                {
                    var qtyString = bulkDiscountLog.AppliedRule.Substring(startIndex, endIndex - startIndex).Trim();

                    if (int.TryParse(qtyString, out int qty))
                    {
                        return qty;
                    }
                }
            }

            return 1;
        }
    }
}