using AutoMapper;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_UnitTemplates;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Mapping
{
    public class UnitTemplateMappingProfile : Profile
    {
        public UnitTemplateMappingProfile()
        {
            // UnitTemplate mappings
            CreateMap<CreateUnitTemplateDto, UnitTemplate>();

            CreateMap<UpdateUnitTemplateDto, UnitTemplate>();

            CreateMap<UnitTemplate, UnitTemplateDto>()
                .ForMember(dest => dest.UnitConversions,
                    opt => opt.MapFrom(src => src.UnitConversions));

            CreateMap<UnitTemplate, UnitTemplateListDto>()
                .ForMember(dest => dest.ConversionsCount,
                    opt => opt.MapFrom(src => src.UnitConversions.Count));

            // UnitConversion mappings
            CreateMap<CreateUnitConversionDto, UnitConversion>();

            CreateMap<UnitConversion, UnitConversionDto>();
        }
    }
}
