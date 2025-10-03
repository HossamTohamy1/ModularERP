using AutoMapper;
using ModularERP.Modules.Inventory.Features.Products.Commends.Commands_ItemGroup;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ItemGroup;
using ModularERP.Modules.Inventory.Features.Products.Models;

namespace ModularERP.Modules.Inventory.Features.Products.Mapping
{
    public class ItemGroupMappingProfile : Profile
    {
        public ItemGroupMappingProfile()
        {
            // Command to Entity
            CreateMap<CreateItemGroupCommand, ItemGroup>();

            CreateMap<UpdateItemGroupCommand, ItemGroup>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<AddItemToGroupCommand, ItemGroupItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // Entity to DTO
            CreateMap<ItemGroup, ItemGroupDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : null))
                .ForMember(dest => dest.ItemsCount, opt => opt.MapFrom(src => src.Items != null ? src.Items.Count : 0));

            CreateMap<ItemGroup, ItemGroupDetailDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : null))
                .ForMember(dest => dest.ItemsCount, opt => opt.MapFrom(src => src.Items != null ? src.Items.Count : 0))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            CreateMap<ItemGroupItem, ItemGroupItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null));
        }
    }
}
