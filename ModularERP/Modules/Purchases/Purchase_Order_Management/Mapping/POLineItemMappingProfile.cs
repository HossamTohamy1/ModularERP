using AutoMapper;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POItme;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Mapping
{
    public class POLineItemMappingProfile : Profile
    {
        public POLineItemMappingProfile()
        {
            // Create mapping
            CreateMap<CreatePOLineItemDto, POLineItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.ReceivedQuantity, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.InvoicedQuantity, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.ReturnedQuantity, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.RemainingQuantity, opt => opt.MapFrom(src => src.Quantity));

            // Update mapping
            CreateMap<UpdatePOLineItemDto, POLineItem>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ReceivedQuantity, opt => opt.Ignore())
                .ForMember(dest => dest.InvoicedQuantity, opt => opt.Ignore())
                .ForMember(dest => dest.ReturnedQuantity, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Response mapping with projection
            CreateMap<POLineItem, POLineItemResponseDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src =>
                    src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src =>
                    src.Service != null ? src.Service.Name : null))
                .ForMember(dest => dest.TaxProfileName, opt => opt.MapFrom(src =>
                    src.TaxProfile != null ? src.TaxProfile.Name : null));

            // List mapping
            CreateMap<POLineItem, POLineItemListDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src =>
                    src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src =>
                    src.Service != null ? src.Service.Name : null));
        }
    }
}
