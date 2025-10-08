using AutoMapper;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Mapping
{
    public class PriceListMappingProfile : Profile
    {
        public PriceListMappingProfile()
        {
            // Create Mapping
            CreateMap<CreatePriceListDto, PriceList>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore())
                .ForMember(dest => dest.Rules, opt => opt.Ignore())
                .ForMember(dest => dest.BulkDiscounts, opt => opt.Ignore())
                .ForMember(dest => dest.Assignments, opt => opt.Ignore());

            // Update Mapping
            CreateMap<UpdatePriceListDto, PriceList>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore())
                .ForMember(dest => dest.Rules, opt => opt.Ignore())
                .ForMember(dest => dest.BulkDiscounts, opt => opt.Ignore())
                .ForMember(dest => dest.Assignments, opt => opt.Ignore());

            // Response Mapping with Projection
            CreateMap<PriceList, PriceListDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : string.Empty))
                .ForMember(dest => dest.CurrencyName,opt => opt.MapFrom(src => src.Currency.Name ?? src.CurrencyCode))
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()));

            // List Mapping with Projection
            CreateMap<PriceList, PriceListListDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : string.Empty))
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}