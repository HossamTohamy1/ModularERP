using AutoMapper;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Command_Custom;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Custom;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Mapping
{
    public class CustomFieldMappingProfile : Profile
    {
        public CustomFieldMappingProfile()
        {
            CreateMap<CustomField, CustomFieldResponseDto>();

            CreateMap<CreateCustomFieldCommand, CustomField>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Common.Enum.Inventory_Enum.CustomFieldStatus.Active));

            CreateMap<UpdateCustomFieldCommand, CustomField>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore());

            CreateMap<CreateCustomFieldDto, CustomField>();
            CreateMap<UpdateCustomFieldDto, CustomField>();
        }
    }
}
