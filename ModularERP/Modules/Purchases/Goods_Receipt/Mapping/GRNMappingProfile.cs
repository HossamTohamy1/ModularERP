using AutoMapper;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Mapping
{
    public class GRNMappingProfile : Profile
    {
        public GRNMappingProfile()
        {
            // Create Mappings
            CreateMap<CreateGRNDto, GoodsReceiptNote>()
                .ForMember(dest => dest.LineItems, opt => opt.MapFrom(src => src.LineItems));

            CreateMap<CreateGRNLineItemDto, GRNLineItem>();

            // Update Mappings
            CreateMap<UpdateGRNDto, GoodsReceiptNote>()
                .ForMember(dest => dest.LineItems, opt => opt.Ignore());

            CreateMap<UpdateGRNLineItemDto, GRNLineItem>();

            // Response Mappings with Projections
            CreateMap<GoodsReceiptNote, GRNResponseDto>()
                .ForMember(dest => dest.PurchaseOrderNumber,
                    opt => opt.MapFrom(src => src.PurchaseOrder.PONumber))
                .ForMember(dest => dest.WarehouseName,
                    opt => opt.MapFrom(src => src.Warehouse.Name))
                .ForMember(dest => dest.CreatedByName,
                    opt => opt.MapFrom(src => src.CreatedByUser != null
                        ? $"{src.CreatedByUser.FirstName} {src.CreatedByUser.LastName}"
                        : null))
                .ForMember(dest => dest.LineItems,
                    opt => opt.MapFrom(src => src.LineItems));

            CreateMap<GRNLineItem, GRNLineItemResponseDto>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.POLineItem.Product.Name))
                .ForMember(dest => dest.ProductSKU,
                    opt => opt.MapFrom(src => src.POLineItem.Product.SKU));

            // List Mappings
            CreateMap<GoodsReceiptNote, GRNListItemDto>()
                .ForMember(dest => dest.PurchaseOrderNumber,
                    opt => opt.MapFrom(src => src.PurchaseOrder.PONumber))
                .ForMember(dest => dest.WarehouseName,
                    opt => opt.MapFrom(src => src.Warehouse.Name))
                .ForMember(dest => dest.LineItemsCount,
                    opt => opt.MapFrom(src => src.LineItems.Count))
                .ForMember(dest => dest.CreatedDate,
                    opt => opt.MapFrom(src => src.CreatedAt));
        }
    }
}