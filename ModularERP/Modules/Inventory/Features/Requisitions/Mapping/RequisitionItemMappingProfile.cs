using AutoMapper;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;
using ModularERP.Modules.Inventory.Features.Requisitions.Models;

public class RequisitionItemMappingProfile : Profile
{
    public RequisitionItemMappingProfile()
    {
        CreateMap<RequisitionItem, RequisitionItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product.SKU))
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.LineTotal));


        CreateMap<CreateRequisitionItemDto, RequisitionItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src =>
                src.UnitPrice.HasValue ? src.UnitPrice.Value * src.Quantity : (decimal?)null));

      
        CreateMap<UpdateRequisitionItemDTO, RequisitionItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RequisitionId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src =>
                src.UnitPrice.HasValue ? src.UnitPrice.Value * src.Quantity : (decimal?)null));
    }
}
