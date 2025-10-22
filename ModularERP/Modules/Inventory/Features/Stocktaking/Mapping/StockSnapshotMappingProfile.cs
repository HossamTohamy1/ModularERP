using AutoMapper;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockSnapshot;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Mapping
{
    public class StockSnapshotMappingProfile : Profile
    {
        public StockSnapshotMappingProfile()
        {
            CreateMap<StockSnapshot, StockSnapshotDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product.SKU));

            CreateMap<StocktakingHeader, SnapshotListDto>()
                .ForMember(dest => dest.StocktakingNumber, opt => opt.MapFrom(src => src.Number))
                .ForMember(dest => dest.SnapshotDate, opt => opt.MapFrom(src => src.DateTime))
                .ForMember(dest => dest.TotalProducts, opt => opt.MapFrom(src => src.Snapshots.Count))
                .ForMember(dest => dest.Snapshots, opt => opt.MapFrom(src => src.Snapshots));
        }
    }
}