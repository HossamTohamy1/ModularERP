using AutoMapper;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ProductStats;
using ModularERP.Modules.Inventory.Features.Products.Models;

namespace ModularERP.Modules.Inventory.Features.Products.Mapping
{
    public class ProductStatsProfile : Profile
    {
        public ProductStatsProfile()
        {
            CreateMap<ProductStats, ProductStatsDto>()
                .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name))
                .ForMember(d => d.ProductSku, opt => opt.MapFrom(s => s.Product.SKU));

            CreateMap<ProductStats, OnHandStockDto>()
                .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name))
                .ForMember(d => d.ProductSku, opt => opt.MapFrom(s => s.Product.SKU))
                .ForMember(d => d.StockStatus, opt => opt.MapFrom(s =>
                    s.OnHandStock <= 0 ? "Out of Stock" :
                    s.OnHandStock < 10 ? "Low Stock" : "In Stock"))
                .ForMember(d => d.AsOfDate, opt => opt.MapFrom(s => s.LastUpdated));

            CreateMap<ProductStats, AverageCostDto>()
                .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name));

            CreateMap<ProductStats, SalesStatsDto>()
                .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name))
                .ForMember(d => d.Last7DaysGrowth, opt => opt.MapFrom(s =>
                    CalculateGrowth(s.SoldLast7Days, s.SoldLast28Days - s.SoldLast7Days)))
                .ForMember(d => d.Last28DaysGrowth, opt => opt.MapFrom(s =>
                    CalculateGrowth(s.SoldLast28Days, s.TotalSold - s.SoldLast28Days)));
        }

        private static decimal CalculateGrowth(decimal current, decimal previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return Math.Round(((current - previous) / previous) * 100, 2);
        }
    }
}
