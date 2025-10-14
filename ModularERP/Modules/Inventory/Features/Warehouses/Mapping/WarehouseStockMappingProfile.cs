using AutoMapper;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO.DTO_WarehouseStock;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Mapping
{
    public class WarehouseStockMappingProfile : Profile
    {
        public WarehouseStockMappingProfile()
        {
            // Entity -> Response DTO
            CreateMap<WarehouseStock, WarehouseStockResponseDto>()
                .ForMember(d => d.WarehouseName, opt => opt.Ignore())
                .ForMember(d => d.ProductName, opt => opt.Ignore())
                .ForMember(d => d.ProductSKU, opt => opt.Ignore());

            // Entity -> List DTO
            CreateMap<WarehouseStock, WarehouseStockListDto>()
                .ForMember(d => d.WarehouseName, opt => opt.Ignore())
                .ForMember(d => d.ProductName, opt => opt.Ignore())
                .ForMember(d => d.ProductSKU, opt => opt.Ignore())
                .ForMember(d => d.IsLowStock, opt => opt.MapFrom(s =>
                    s.MinStockLevel.HasValue && s.Quantity <= s.MinStockLevel.Value));

            // Create DTO -> Entity
            CreateMap<CreateWarehouseStockDto, WarehouseStock>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.AvailableQuantity, opt => opt.MapFrom(s =>
                    s.Quantity - (s.ReservedQuantity ?? 0)))
                .ForMember(d => d.TotalValue, opt => opt.MapFrom(s =>
                    s.Quantity * (s.AverageUnitCost ?? 0)))
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
                .ForMember(d => d.CreatedById, opt => opt.Ignore())
                .ForMember(d => d.UpdatedById, opt => opt.Ignore())
                .ForMember(d => d.IsDeleted, opt => opt.Ignore())
                .ForMember(d => d.LastStockInDate, opt => opt.Ignore())
                .ForMember(d => d.LastStockOutDate, opt => opt.Ignore());

            // Update DTO -> Entity
            CreateMap<UpdateWarehouseStockDto, WarehouseStock>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.WarehouseId, opt => opt.Ignore())
                .ForMember(d => d.ProductId, opt => opt.Ignore())
                .ForMember(d => d.AvailableQuantity, opt => opt.MapFrom(s =>
                    s.Quantity - (s.ReservedQuantity ?? 0)))
                .ForMember(d => d.TotalValue, opt => opt.MapFrom(s =>
                    s.Quantity * (s.AverageUnitCost ?? 0)))
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
                .ForMember(d => d.CreatedById, opt => opt.Ignore())
                .ForMember(d => d.UpdatedById, opt => opt.Ignore())
                .ForMember(d => d.IsDeleted, opt => opt.Ignore())
                .ForMember(d => d.LastStockInDate, opt => opt.Ignore())
                .ForMember(d => d.LastStockOutDate, opt => opt.Ignore());
        }
    }
}