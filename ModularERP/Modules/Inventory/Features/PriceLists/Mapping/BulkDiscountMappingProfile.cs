using AutoMapper;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_BulkDiscount;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_BulkDiscount;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Mapping
{
    public class BulkDiscountMappingProfile : Profile
    {
        public BulkDiscountMappingProfile()
        {
            // Entity to DTO - Using Projection (No Include)
            CreateMap<BulkDiscount, BulkDiscountDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product.SKU))
                .ForMember(dest => dest.PriceListName, opt => opt.MapFrom(src => src.PriceList.Name));

            // Command to Entity
            CreateMap<CreateBulkDiscountCommand, BulkDiscount>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.PriceList, opt => opt.Ignore());

            CreateMap<UpdateBulkDiscountCommand, BulkDiscount>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PriceListId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.PriceList, opt => opt.Ignore());
        }
    }
}
