using AutoMapper;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceList;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Mapping
{
    public class PriceListItemMappingProfile : Profile
    {
        public PriceListItemMappingProfile()
        {
            // Create Mapping
            CreateMap<CreatePriceListItemDto, PriceListItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PriceListId, opt => opt.Ignore())
                .ForMember(dest => dest.FinalPrice, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.PriceList, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.Service, opt => opt.Ignore())
                .ForMember(dest => dest.TaxProfile, opt => opt.Ignore());

            // Update Mapping
            CreateMap<UpdatePriceListItemDto, PriceListItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PriceListId, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceId, opt => opt.Ignore())
                .ForMember(dest => dest.FinalPrice, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.PriceList, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.Service, opt => opt.Ignore())
                .ForMember(dest => dest.TaxProfile, opt => opt.Ignore());

            // Response Mapping with Projection
            CreateMap<PriceListItem, PriceListItemDto>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.ProductSKU,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : null))
                .ForMember(dest => dest.ServiceName,
                    opt => opt.MapFrom(src => src.Service != null ? src.Service.Name : null))
                .ForMember(dest => dest.ServiceCode,
                    opt => opt.MapFrom(src => src.Service != null ? src.Service.Code : null))
                .ForMember(dest => dest.TaxProfileName,
                    opt => opt.MapFrom(src => src.TaxProfile != null ? src.TaxProfile.Name : null));

            // List Mapping with Projection
            CreateMap<PriceListItem, PriceListItemListDto>()
                .ForMember(dest => dest.ItemName,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.Name :
                                              src.Service != null ? src.Service.Name : string.Empty))
                .ForMember(dest => dest.ItemCode,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU :
                                              src.Service != null ? src.Service.Code : string.Empty))
                .ForMember(dest => dest.ItemType,
                    opt => opt.MapFrom(src => src.Product != null ? "Product" :
                                              src.Service != null ? "Service" : "Unknown"));
        }
    }
}