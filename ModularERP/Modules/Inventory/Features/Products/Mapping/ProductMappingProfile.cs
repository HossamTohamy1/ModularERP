using AutoMapper;
using ModularERP.Modules.Inventory.Features.Products.DTO;
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
                .ForMember(dest => dest.ProfitMargin, opt => opt.MapFrom(src => CalculateProfitMargin(src.PurchasePrice, src.SellingPrice)));

            // ========== Product -> ProductListItemDto ==========
            CreateMap<Product, ProductListItemDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.Ignore()) // Will be populated from join
                .ForMember(dest => dest.BrandName, opt => opt.Ignore())    // Will be populated from join
                .ForMember(dest => dest.OnHandStock, opt => opt.Ignore()); // Will be populated from ProductStats

            // ========== Product -> ProductDetailsDto ==========
            CreateMap<Product, ProductDetailsDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.Ignore())    // Will be populated from join
                .ForMember(dest => dest.BrandName, opt => opt.Ignore())       // Will be populated from join
                .ForMember(dest => dest.SupplierName, opt => opt.Ignore())    // Will be populated from join
                .ForMember(dest => dest.ItemGroupName, opt => opt.Ignore())   // Will be populated from join
                .ForMember(dest => dest.Stats, opt => opt.Ignore());          // Will be populated separately

            // ========== ProductStats -> ProductStatsDto ==========
            CreateMap<ProductStats, ProductStatsDto>()
                .ForMember(dest => dest.StockStatus, opt => opt.Ignore()); // Calculated in handler
        }

        // Helper method to calculate profit margin
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

