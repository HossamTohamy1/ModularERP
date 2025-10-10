using AutoMapper;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceCalculation;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Mappings
{
    public class PriceCalculationMappingProfile : Profile
    {
        public PriceCalculationMappingProfile()
        {
            // PriceCalculationLog -> PriceCalculationLogDTO
            CreateMap<PriceCalculationLog, PriceCalculationLogDTO>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.ProductSKU,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty));

            // PriceCalculationLog -> RuleApplicationDTO
            CreateMap<PriceCalculationLog, RuleApplicationDTO>()
                .ForMember(dest => dest.RuleName,
                    opt => opt.MapFrom(src => src.AppliedRule))
                .ForMember(dest => dest.RuleType,
                    opt => opt.MapFrom(src => ExtractRuleType(src.AppliedRule)))
                .ForMember(dest => dest.PriceBefore,
                    opt => opt.MapFrom(src => src.ValueBefore ?? 0))
                .ForMember(dest => dest.PriceAfter,
                    opt => opt.MapFrom(src => src.ValueAfter ?? 0))
                .ForMember(dest => dest.Impact,
                    opt => opt.MapFrom(src => (src.ValueAfter ?? 0) - (src.ValueBefore ?? 0)))
                .ForMember(dest => dest.RuleValue,
                    opt => opt.Ignore());
        }

        private string ExtractRuleType(string? appliedRule)
        {
            if (string.IsNullOrEmpty(appliedRule)) return "Unknown";
            if (appliedRule.Contains("Markup")) return "Markup";
            if (appliedRule.Contains("Margin")) return "Margin";
            if (appliedRule.Contains("Fixed")) return "FixedAdjustment";
            if (appliedRule.Contains("Bulk Discount")) return "BulkDiscount";
            return "Custom";
        }
    }
}