using AutoMapper;
using ModularERP.Modules.Inventory.Features.TaxManagement.Commends;
using ModularERP.Modules.Inventory.Features.TaxManagement.DTO;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Mapping
{
    public class TaxProfileMappingProfile : Profile
    {
        public TaxProfileMappingProfile()
        {
            // TaxProfile mappings
            CreateMap<TaxProfile, TaxProfileDto>();

            CreateMap<TaxProfile, TaxProfileDetailDto>()
                .ForMember(dest => dest.Components, opt => opt.Ignore());

            CreateMap<CreateTaxProfileCommand, TaxProfile>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.TaxProfileComponents, opt => opt.Ignore());

            CreateMap<UpdateTaxProfileCommand, TaxProfile>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.TaxProfileComponents, opt => opt.Ignore());

            // TaxComponent mappings
            CreateMap<TaxComponent, TaxComponentDto>()
                .ForMember(dest => dest.RateType, opt => opt.MapFrom(src => src.RateType.ToString()))
                .ForMember(dest => dest.IncludedType, opt => opt.MapFrom(src => src.IncludedType.ToString()))
                .ForMember(dest => dest.AppliesOn, opt => opt.MapFrom(src => src.AppliesOn.ToString()));

            // TaxProfileComponent mappings
            CreateMap<TaxProfileComponent, TaxProfileComponentDto>()
                .ForMember(dest => dest.ComponentName, opt => opt.Ignore())
                .ForMember(dest => dest.RateType, opt => opt.Ignore())
                .ForMember(dest => dest.RateValue, opt => opt.Ignore())
                .ForMember(dest => dest.IncludedType, opt => opt.Ignore())
                .ForMember(dest => dest.AppliesOn, opt => opt.Ignore());

            // TaxComponent Command mappings
            CreateMap<CreateTaxComponentCommand, TaxComponent>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.TaxProfileComponents, opt => opt.Ignore());

            CreateMap<UpdateTaxComponentCommand, TaxComponent>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.TaxProfileComponents, opt => opt.Ignore());
        }
    }
}
