using AutoMapper;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Mapping
{
    public class PurchaseOrderMappingProfile : Profile
    {
        public PurchaseOrderMappingProfile()
        {
            // PurchaseOrder mappings
            CreateMap<PurchaseOrder, CreatePurchaseOrderDto>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.Name));

            CreateMap<PurchaseOrder, PurchaseOrderDetailDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.Name))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.Name))
                .ForMember(dest => dest.LineItems, opt => opt.MapFrom(src => src.LineItems))
                .ForMember(dest => dest.Deposits, opt => opt.MapFrom(src => src.Deposits))
                .ForMember(dest => dest.ShippingCharges, opt => opt.MapFrom(src => src.ShippingCharges))
                .ForMember(dest => dest.Discounts, opt => opt.MapFrom(src => src.Discounts))
                .ForMember(dest => dest.Adjustments, opt => opt.MapFrom(src => src.Adjustments))
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments))
                .ForMember(dest => dest.PONotes, opt => opt.MapFrom(src => src.PONotes));

            CreateMap<PurchaseOrder, PurchaseOrderListDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.Name))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.Name))
                .ForMember(dest => dest.LineItemsCount, opt => opt.MapFrom(src => src.LineItems.Count));

            // POLineItem mappings
            CreateMap<POLineItem, POLineItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service != null ? src.Service.Name : null))
                .ForMember(dest => dest.TaxProfileName, opt => opt.MapFrom(src => src.TaxProfile != null ? src.TaxProfile.Name : null));

            // PODeposit mappings
            CreateMap<PODeposit, PODepositDto>();

            // POShippingCharge mappings
            CreateMap<POShippingCharge, POShippingChargeDto>()
                .ForMember(dest => dest.TaxProfileName, opt => opt.MapFrom(src => src.TaxProfile != null ? src.TaxProfile.Name : null));

            // PODiscount mappings
            CreateMap<PODiscount, PODiscountDto>();

            // POAdjustment mappings
            CreateMap<POAdjustment, POAdjustmentDto>();

            // POAttachment mappings
            CreateMap<POAttachment, POAttachmentDto>()
                .ForMember(dest => dest.UploadedByName, opt => opt.MapFrom(src =>
                    src.UploadedByUser != null
                        ? $"{src.UploadedByUser.FirstName} {src.UploadedByUser.LastName}".Trim()
                        : "Unknown"));

            // PONote mappings
            CreateMap<PONote, PONoteDto>();
        }
    }
}