using AutoMapper;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundItem;
using ModularERP.Modules.Purchases.Refunds.Models;

namespace ModularERP.Modules.Purchases.Refunds.Mapping
{
    public class RefundItemMappingProfile : Profile
    {
        public RefundItemMappingProfile()
        {
            // RefundLineItem -> RefundItemDto
            CreateMap<RefundLineItem, RefundItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.GRNLineItem.POLineItem.Product.Name))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.GRNLineItem.POLineItem.Product.SKU));

            // RefundLineItem -> RefundItemListDto
            CreateMap<RefundLineItem, RefundItemListDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.GRNLineItem.POLineItem.Product.Name));
        }
    }
}

