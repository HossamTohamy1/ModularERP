using AutoMapper;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Mapping
{
    public class StocktakingMappingProfile : Profile
    {
        public StocktakingMappingProfile()
        {
            // Command to Entity
            CreateMap<CreateStocktakingCommand, StocktakingHeader>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Common.Enum.Inventory_Enum.StocktakingStatus.Draft))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            CreateMap<UpdateStocktakingCommand, StocktakingHeader>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // Entity to DTO (for AutoMapper projection support)
            CreateMap<StocktakingHeader, CreateStocktakingDto>()
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : string.Empty));

            CreateMap<StocktakingHeader, UpdateStocktakingDto>()
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty));

            CreateMap<StocktakingHeader, StocktakingListDto>()
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : string.Empty))
                .ForMember(dest => dest.LineCount, opt => opt.MapFrom(src => src.Lines.Count))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedByUser != null ? src.ApprovedByUser.UserName : null));

            CreateMap<StocktakingHeader, StocktakingDetailDto>()
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : string.Empty))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedByUser != null ? src.ApprovedByUser.UserName : null))
                .ForMember(dest => dest.PostedByName, opt => opt.MapFrom(src => src.PostedByUser != null ? src.PostedByUser.UserName : null))
                .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.Lines));

            CreateMap<StocktakingLine, StocktakingLineDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));
        }
    }
}