using AutoMapper;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_Invoice;
using ModularERP.Modules.Purchases.Invoicing.Models;

namespace ModularERP.Modules.Purchases.Invoicing.Mapping
{
    public class PurchaseInvoiceMappingProfile : Profile
    {
        public PurchaseInvoiceMappingProfile()
        {
            // PurchaseInvoice -> PurchaseInvoiceDto with projection
            CreateMap<PurchaseInvoice, PurchaseInvoiceDto>()
                .ForMember(dest => dest.PurchaseOrderNumber,
                    opt => opt.MapFrom(src => src.PurchaseOrder != null ? src.PurchaseOrder.PONumber : string.Empty))
                .ForMember(dest => dest.CompanyName,
                    opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : string.Empty))
                .ForMember(dest => dest.SupplierName,
                    opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
                .ForMember(dest => dest.LineItems,
                    opt => opt.MapFrom(src => src.LineItems))
                .ForMember(dest => dest.Payments,
                    opt => opt.MapFrom(src => src.Payments));

            // InvoiceLineItem -> InvoiceLineItemDto
            CreateMap<InvoiceLineItem, InvoiceLineItemDto>();

            // SupplierPayment -> SupplierPaymentDto
            CreateMap<SupplierPayment, SupplierPaymentDto>();

            // CreateInvoiceLineItemDto -> InvoiceLineItem
            CreateMap<CreateInvoiceLineItemDto, InvoiceLineItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore());

            // UpdateInvoiceLineItemDto -> InvoiceLineItem
            CreateMap<UpdateInvoiceLineItemDto, InvoiceLineItem>()
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore());
        }
    }
}
