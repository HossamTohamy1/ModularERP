using AutoMapper;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.Models;

namespace ModularERP.Modules.Purchases.Refunds.Mapping
{
    public class RefundMappingProfile : Profile
    {
        public RefundMappingProfile()
        {
            // PurchaseRefund to RefundDto
            CreateMap<PurchaseRefund, RefundDto>()
                .ForMember(dest => dest.PurchaseOrderNumber,
                    opt => opt.MapFrom(src => src.PurchaseOrder.PONumber))
                .ForMember(dest => dest.SupplierName,
                    opt => opt.MapFrom(src => src.Supplier.Name))
                .ForMember(dest => dest.HasDebitNote,
                    opt => opt.MapFrom(src => src.DebitNote != null))
                .ForMember(dest => dest.DebitNote,
                    opt => opt.MapFrom(src => src.DebitNote))
                .ForMember(dest => dest.LineItems,
                    opt => opt.MapFrom(src => src.LineItems));

            // RefundLineItem to RefundLineItemDto
            // Fixed: Navigate through GRNLineItem.POLineItem.Product
            CreateMap<RefundLineItem, RefundLineItemDto>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.GRNLineItem.POLineItem.Product.Name))
                .ForMember(dest => dest.ProductSKU,
                    opt => opt.MapFrom(src => src.GRNLineItem.POLineItem.Product.SKU));

            // DebitNote to DebitNoteDto
            CreateMap<DebitNote, DebitNoteDto>()
                .ForMember(dest => dest.SupplierName,
                    opt => opt.MapFrom(src => src.Supplier.Name));

            // CreateRefundCommand to PurchaseRefund
            CreateMap<CreateRefundCommand, PurchaseRefund>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RefundNumber, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LineItems, opt => opt.Ignore());

            // CreateRefundLineItemDto to RefundLineItem
            CreateMap<CreateRefundLineItemDto, RefundLineItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RefundId, opt => opt.Ignore())
                .ForMember(dest => dest.LineTotal,
                    opt => opt.MapFrom(src => src.ReturnQuantity * src.UnitPrice))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        }
    }
}