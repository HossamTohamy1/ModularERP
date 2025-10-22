using AutoMapper;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_WorkFlow;
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

            // ========== DTO_StockTaking_Header namespace mappings ==========

            CreateMap<StocktakingHeader, DTO.DTO_StockTaking_Header.CreateStocktakingDto>()
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : string.Empty));

            CreateMap<StocktakingHeader, DTO.DTO_StockTaking_Header.UpdateStocktakingDto>()
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty));

            CreateMap<StocktakingHeader, DTO.DTO_StockTaking_Header.StocktakingListDto>()
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : string.Empty))
                .ForMember(dest => dest.LineCount, opt => opt.MapFrom(src => src.Lines.Count))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedByUser != null ? src.ApprovedByUser.UserName : null));

            CreateMap<StocktakingHeader, DTO.DTO_StockTaking_Header.StocktakingDetailDto>()
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : string.Empty))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedByUser != null ? src.ApprovedByUser.UserName : null))
                .ForMember(dest => dest.PostedByName, opt => opt.MapFrom(src => src.PostedByUser != null ? src.PostedByUser.UserName : null))
                .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.Lines));

            CreateMap<StocktakingLine, DTO.DTO_StockTaking_Header.StocktakingLineDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));

            // ========== DTO_StockTaking_WorkFlow namespace mappings ==========

            CreateMap<StocktakingHeader, DTO.DTO_StockTaking_WorkFlow.StartStocktakingDto>()
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.Number))
                .ForMember(dest => dest.WarehouseId, opt => opt.MapFrom(src => src.WarehouseId))
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.LineCount, opt => opt.MapFrom(src => src.Lines.Count))
                .ForMember(dest => dest.DateTime, opt => opt.MapFrom(src => src.DateTime));

            CreateMap<StocktakingHeader, DTO.DTO_StockTaking_WorkFlow.ReviewStocktakingDto>()
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.Number))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.RecordedItems, opt => opt.MapFrom(src => src.Lines.Count))
                .ForMember(dest => dest.TotalVarianceValue, opt => opt.MapFrom(src =>
                    src.Lines.Sum(l => Math.Abs(l.VarianceValue ?? 0))))
                .ForMember(dest => dest.DateTime, opt => opt.MapFrom(src => src.DateTime));

            CreateMap<StocktakingHeader, DTO.DTO_StockTaking_WorkFlow.PostStocktakingDto>()
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.Number))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PostedAt, opt => opt.MapFrom(src => src.PostedAt ?? DateTime.UtcNow))
                .ForMember(dest => dest.AdjustmentsPosted, opt => opt.MapFrom(src =>
                    src.Lines.Count(l => (l.VarianceQty ?? 0) != 0)))
                .ForMember(dest => dest.TotalAdjustmentValue, opt => opt.MapFrom(src =>
                    src.Lines.Sum(l => Math.Abs(l.VarianceValue ?? 0))));

            CreateMap<StocktakingHeader, DTO.DTO_StockTaking_WorkFlow.UpdateStocktakingDto>()
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.Number))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.UpdateSystem, opt => opt.MapFrom(src => src.UpdateSystem))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt ?? DateTime.UtcNow));

            CreateMap<StocktakingLine, DTO.DTO_StockTaking_WorkFlow.StocktakingLineDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.ImagePath));
        }
    }
}