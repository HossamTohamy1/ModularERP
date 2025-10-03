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
            // ========== CreateProductDto -> Product ==========
            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.PhotoUrl))
                .ForMember(dest => dest.ProfitMargin, opt => opt.MapFrom(src => CalculateProfitMargin(src.PurchasePrice, src.SellingPrice)));

            // ========== UpdateProductDto -> Product ==========
            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.PhotoUrl))
                .ForMember(dest => dest.ProfitMargin, opt => opt.MapFrom(src => CalculateProfitMargin(src.PurchasePrice, src.SellingPrice)))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // ========== Product -> ProductListItemDto ==========
            CreateMap<Product, ProductListItemDto>()
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photo))
                .ForMember(dest => dest.CategoryName, opt => opt.Ignore())
                .ForMember(dest => dest.BrandName, opt => opt.Ignore())
                .ForMember(dest => dest.OnHandStock, opt => opt.Ignore());

            // ========== Product -> ProductDetailsDto ==========
            CreateMap<Product, ProductDetailsDto>()
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photo))
                .ForMember(dest => dest.CategoryName, opt => opt.Ignore())
                .ForMember(dest => dest.BrandName, opt => opt.Ignore())
                .ForMember(dest => dest.SupplierName, opt => opt.Ignore())
                .ForMember(dest => dest.ItemGroupName, opt => opt.Ignore())
                .ForMember(dest => dest.Stats, opt => opt.Ignore());

            // ========== ProductStats -> ProductStatsDto ==========
            CreateMap<ProductStats, ProductStatsDto>()
                .ForMember(dest => dest.AverageUnitCost, opt => opt.MapFrom(src => src.AvgUnitCost))
                .ForMember(dest => dest.StockStatus, opt => opt.Ignore());
        }

        private static decimal? CalculateProfitMargin(decimal purchasePrice, decimal sellingPrice)
        {
            if (sellingPrice > 0 && purchasePrice > 0)
            {
                return ((sellingPrice - purchasePrice) / sellingPrice) * 100;
            }
            return null;
        }
    }
}