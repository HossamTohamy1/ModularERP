using AutoMapper;
using ModularERP.Modules.Inventory.Features.Products.DTO;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_Product;
using ModularERP.Modules.Inventory.Features.Products.Models;

namespace ModularERP.Modules.Inventory.Features.Products.Mapping
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            // CreateProductDto -> Product
            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.PhotoUrl))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    Enum.Parse<Common.Enum.Inventory_Enum.ProductStatus>(src.Status)))
                .ForMember(dest => dest.DiscountType, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.DiscountType)
                        ? (Common.Enum.Inventory_Enum.DiscountType?)null
                        : Enum.Parse<Common.Enum.Inventory_Enum.DiscountType>(src.DiscountType)))
                .ForMember(dest => dest.MinPrice, opt => opt.MapFrom(src => src.MinimumPrice))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.Warehouse, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.Supplier, opt => opt.Ignore())
                .ForMember(dest => dest.ProductTaxProfiles, opt => opt.Ignore())
                .ForMember(dest => dest.ItemGroupItems, opt => opt.Ignore())
                .ForMember(dest => dest.CustomFieldValues, opt => opt.Ignore());

            // Product -> ProductDetailsDto
            CreateMap<Product, ProductDetailsDto>()
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photo))
                .ForMember(dest => dest.MinimumPrice, opt => opt.MapFrom(src => src.MinPrice))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.DiscountType, opt => opt.MapFrom(src =>
                    src.DiscountType.HasValue ? src.DiscountType.Value.ToString() : null))
                .ForMember(dest => dest.CategoryName, opt => opt.Ignore())
                .ForMember(dest => dest.BrandName, opt => opt.Ignore())
                .ForMember(dest => dest.SupplierName, opt => opt.Ignore())
                .ForMember(dest => dest.WarehouseName, opt => opt.Ignore())
                .ForMember(dest => dest.ItemGroupId, opt => opt.Ignore())
                .ForMember(dest => dest.ItemGroupName, opt => opt.Ignore())
                .ForMember(dest => dest.Stats, opt => opt.Ignore());

            // Product -> ProductListItemDto
            CreateMap<Product, ProductListItemDto>()
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photo))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.CategoryName, opt => opt.Ignore())
                .ForMember(dest => dest.BrandName, opt => opt.Ignore())
                .ForMember(dest => dest.WarehouseName, opt => opt.Ignore())
                .ForMember(dest => dest.OnHandStock, opt => opt.Ignore());

            // ProductStats -> ProductStatsDto
            CreateMap<ProductStats, ProductStatsDto>()
                .ForMember(dest => dest.AverageUnitCost, opt => opt.MapFrom(src => src.AvgUnitCost))
                .ForMember(dest => dest.StockStatus, opt => opt.Ignore());
        }
    }
}