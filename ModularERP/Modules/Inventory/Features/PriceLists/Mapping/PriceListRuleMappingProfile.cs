using AutoMapper;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceRule;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Mapping
{
    public class PriceListRuleMappingProfile : Profile
    {
        public PriceListRuleMappingProfile()
        {
            // Entity to Response DTO
            CreateMap<PriceListRule, PriceListRuleResponseDTO>()
                .ForMember(dest => dest.RuleTypeName,
                    opt => opt.MapFrom(src => src.RuleType.ToString()));

            // Create DTO to Entity
            CreateMap<CreatePriceListRuleDTO, PriceListRule>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PriceListId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.PriceList, opt => opt.Ignore());

            // Update DTO to Entity
            CreateMap<UpdatePriceListRuleDTO, PriceListRule>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PriceListId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.PriceList, opt => opt.Ignore());
        }
    }
}