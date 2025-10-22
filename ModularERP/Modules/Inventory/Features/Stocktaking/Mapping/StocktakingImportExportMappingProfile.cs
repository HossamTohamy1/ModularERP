using AutoMapper;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_ImportExport;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Mapping
{
    public class StocktakingImportExportMappingProfile : Profile
    {
        public StocktakingImportExportMappingProfile()
        {
            // Imported Line Mapping
            CreateMap<StocktakingLine, ImportedLineDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty))
                .ForMember(dest => dest.LineId, opt => opt.MapFrom(src => src.Id));
        }
    }
}