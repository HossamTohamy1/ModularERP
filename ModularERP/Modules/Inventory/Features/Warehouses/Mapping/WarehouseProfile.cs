using AutoMapper;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Modules.Inventory.Features.Warehouses.Commends;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Validators;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Mapping
{
    public class WarehouseProfile : Profile
    {
        public WarehouseProfile()
        {
            CreateMap<Warehouse, WarehouseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<Warehouse, WarehouseListDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CreateWarehouseDto, CreateWarehouseCommand>();

            CreateMap<UpdateWarehouseDto, UpdateWarehouseCommand>();

            // Mapping from Command to Entity
            CreateMap<CreateWarehouseCommand, Warehouse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<WarehouseStatus>(src.Status)))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore());

            CreateMap<UpdateWarehouseCommand, Warehouse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<WarehouseStatus>(src.Status)))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore());

            // Mapping for PagedResult
            CreateMap<Warehouse, WarehouseListDto>();

            CreateMap<object, PagedResult<WarehouseListDto>>()
                .ConstructUsing((src, ctx) =>
                {
                    var source = src as dynamic;
                    var items = ctx.Mapper.Map<List<WarehouseListDto>>(source.Warehouses);
                    return new PagedResult<WarehouseListDto>
                    {
                        Items = items,
                        TotalCount = source.TotalCount,
                        PageNumber = source.PageNumber,
                        PageSize = source.PageSize
                    };
                });
        }
    }
}