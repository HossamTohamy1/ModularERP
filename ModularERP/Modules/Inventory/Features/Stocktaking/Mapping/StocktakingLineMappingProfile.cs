using AutoMapper;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Line;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Mapping
{
    public class StocktakingLineMappingProfile : Profile
    {
        public StocktakingLineMappingProfile()
        {
            CreateMap<StocktakingLine, StocktakingLineDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product.SKU))
                .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.UnitId != null ? "Unit" : null));

            CreateMap<CreateStocktakingLineDto, StocktakingLine>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.SystemQtySnapshot, opt => opt.Ignore())
                .ForMember(dest => dest.SystemQtyAtPost, opt => opt.Ignore())
                .ForMember(dest => dest.VarianceQty, opt => opt.Ignore())
                .ForMember(dest => dest.ValuationCost, opt => opt.Ignore())
                .ForMember(dest => dest.VarianceValue, opt => opt.Ignore())
                .ForMember(dest => dest.Stocktaking, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore());
        }
    }
}